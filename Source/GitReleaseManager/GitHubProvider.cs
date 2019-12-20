//-----------------------------------------------------------------------
// <copyright file="GitHubProvider.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Extensions;
    using Octokit;
    using Issue = GitReleaseManager.Core.Model.Issue;
    using Milestone = GitReleaseManager.Core.Model.Milestone;
    using Release = GitReleaseManager.Core.Model.Release;

    public class GitHubProvider : IVcsProvider
    {
        private readonly Config _configuration;
        private readonly IMapper _mapper;
        private GitHubClient _gitHubClient;

        public GitHubProvider(IMapper mapper, Config configuration, string userName, string password, string token)
        {
            _mapper = mapper;
            _configuration = configuration;
            CreateClient(userName, password, token);
        }

        public async Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone, string user, string repository)
        {
            if (currentMilestone is null)
            {
                throw new ArgumentNullException(nameof(currentMilestone));
            }

            try
            {
                if (previousMilestone == null)
                {
                    var gitHubClientRepositoryCommitsCompare = await _gitHubClient.Repository.Commit.Compare(user, repository, "master", currentMilestone.Title).ConfigureAwait(false);
                    return gitHubClientRepositoryCommitsCompare.AheadBy;
                }

                var compareResult = await _gitHubClient.Repository.Commit.Compare(user, repository, previousMilestone.Title, "master").ConfigureAwait(false);
                return compareResult.AheadBy;
            }
            catch (NotFoundException)
            {
                Logger.WriteWarning("Unable to find tag for milestone, so commit count will be returned as zero");

                // If there is no tag yet the Compare will return a NotFoundException
                // we can safely ignore
                return 0;
            }
        }

        public async Task<List<Issue>> GetIssuesAsync(Milestone targetMilestone)
        {
            var githubMilestone = _mapper.Map<Octokit.Milestone>(targetMilestone);
            var allIssues = await _gitHubClient.AllIssuesForMilestone(githubMilestone).ConfigureAwait(false);
            return _mapper.Map<List<Issue>>(allIssues.Where(x => x.State == ItemState.Closed).ToList());
        }

        public async Task<List<Release>> GetReleasesAsync(string user, string repository)
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(user, repository).ConfigureAwait(false);
            return _mapper.Map<List<Release>>(allReleases.OrderByDescending(r => r.CreatedAt).ToList());
        }

        public async Task<Release> GetSpecificRelease(string tagName, string user, string repository)
        {
            return _mapper.Map<Release>(await GetReleaseFromTagNameAsync(user, repository, tagName).ConfigureAwait(false));
        }

        public ReadOnlyCollection<Milestone> GetReadOnlyMilestones(string user, string repository)
        {
            var milestonesClient = _gitHubClient.Issue.Milestone;
            var closed = milestonesClient.GetAllForRepository(
                user,
                repository,
                new MilestoneRequest
                {
                    State = ItemStateFilter.Closed,
                }).Result;

            var open = milestonesClient.GetAllForRepository(
                user,
                repository,
                new MilestoneRequest
                {
                    State = ItemStateFilter.Open,
                }).Result;

            return new ReadOnlyCollection<Milestone>(_mapper.Map<List<Milestone>>(closed.Concat(open).ToList()));
        }

        public void CreateClient(string userName, string password, string token)
        {
            var credentials = string.IsNullOrWhiteSpace(token)
                ? new Credentials(userName, password)
                : new Credentials(token);

            var github = new GitHubClient(new ProductHeaderValue("GitReleaseManager")) { Credentials = credentials };
            _gitHubClient = github;
        }

        public string GetCommitsLink(string user, string repository, Milestone milestone, Milestone previousMilestone)
        {
            if (milestone is null)
            {
                throw new ArgumentNullException(nameof(milestone));
            }

            if (previousMilestone is null)
            {
                return string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/commits/{2}", user, repository, milestone.Title);
            }

            return string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/compare/{2}...{3}", user, repository, previousMilestone.Title, milestone.Title);
        }

        public async Task<Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease)
        {
            var release = await GetReleaseFromTagNameAsync(owner, repository, milestone).ConfigureAwait(false);
            var releaseNotesBuilder = new ReleaseNotesBuilder(this, owner, repository, milestone, _configuration);
            var result = await releaseNotesBuilder.BuildReleaseNotes().ConfigureAwait(false);

            if (release == null)
            {
                var releaseUpdate = CreateNewRelease(releaseName, milestone, result, prerelease, targetCommitish);
                release = await _gitHubClient.Repository.Release.Create(owner, repository, releaseUpdate).ConfigureAwait(false);
            }
            else
            {
                Logger.WriteWarning(string.Format("A release for milestone {0} already exists, and will be updated", milestone));

                if (!release.Draft)
                {
                    throw new Exception("Release is not in draft state, so not updating.");
                }

                var releaseUpdate = release.ToUpdate();
                releaseUpdate.Body = result;
                await _gitHubClient.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate).ConfigureAwait(false);
            }

            await AddAssets(owner, repository, milestone, assets).ConfigureAwait(false);
            return _mapper.Map<Octokit.Release, Release>(release);
        }

        public async Task<Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new ArgumentException("Unable to locate input file.");
            }

            var inputFileContents = File.ReadAllText(inputFilePath);

            var releaseUpdate = CreateNewRelease(name, name, inputFileContents, prerelease, targetCommitish);

            var release = await _gitHubClient.Repository.Release.Create(owner, repository, releaseUpdate).ConfigureAwait(false);

            await AddAssets(owner, repository, name, assets).ConfigureAwait(false);

            return _mapper.Map<Octokit.Release, Release>(release);
        }

        public async Task DiscardRelease(string owner, string repository, string name)
        {
            var release = await GetReleaseFromTagNameAsync(owner, repository, name).ConfigureAwait(false);

            if (release == null)
            {
                throw new Exception(string.Format("Unable to find a release with name {0}", name));
            }

            if (!release.Draft)
            {
                throw new Exception("Release is not in draft state, so not discarding.");
            }

            await _gitHubClient.Repository.Release.Delete(owner, repository, release.Id).ConfigureAwait(false);
            return;
        }

        public async Task AddAssets(string owner, string repository, string tagName, IList<string> assets)
        {
            var release = await GetReleaseFromTagNameAsync(owner, repository, tagName).ConfigureAwait(false);

            if (release == null)
            {
                Logger.WriteError("Unable to find Release with specified tagName");
                return;
            }

            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    if (!File.Exists(asset))
                    {
                        Logger.WriteWarning(string.Format("Requested asset to be uploaded doesn't exist: {0}", asset));
                        continue;
                    }

                    var assetFileName = Path.GetFileName(asset);

                    var existingAsset = release.Assets.Where(a => a.Name == assetFileName).FirstOrDefault();
                    if (existingAsset != null)
                    {
                        Logger.WriteWarning(string.Format("Requested asset to be uploaded already exists on draft release, replacing with new file: {0}", asset));
                        await _gitHubClient.Repository.Release.DeleteAsset(owner, repository, existingAsset.Id).ConfigureAwait(false);
                    }

                    var upload = new ReleaseAssetUpload
                    {
                        FileName = assetFileName,
                        ContentType = "application/octet-stream",
                        RawData = File.Open(asset, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                    };

                    await _gitHubClient.Repository.Release.UploadAsset(release, upload).ConfigureAwait(false);

                    // Make sure to tidy up the stream that was created above
                    upload.RawData.Dispose();
                }

                if (assets != null && assets.Any() && _configuration.Create.IncludeShaSection)
                {
                    var stringBuilder = new StringBuilder(release.Body);

                    if (!release.Body.Contains(_configuration.Create.ShaSectionHeading))
                    {
                        stringBuilder.AppendLine(string.Format("### {0}", _configuration.Create.ShaSectionHeading));
                    }

                    foreach (var asset in assets)
                    {
                        var file = new FileInfo(asset);

                        if (!file.Exists)
                        {
                            continue;
                        }

                        stringBuilder.AppendFormat(_configuration.Create.ShaSectionLineFormat, file.Name, ComputeSha256Hash(asset));
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.AppendLine();

                    var releaseUpdate = release.ToUpdate();
                    releaseUpdate.Body = stringBuilder.ToString();
                    await _gitHubClient.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate).ConfigureAwait(false);
                }
            }
        }

        public Task<string> ExportReleases(string owner, string repository, string tagName)
        {
            var releaseNotesExporter = new ReleaseNotesExporter(this, _configuration, owner, repository);

            return releaseNotesExporter.ExportReleaseNotes(tagName);
        }

        public async Task CloseMilestone(string owner, string repository, string milestoneTitle)
        {
            var milestoneClient = _gitHubClient.Issue.Milestone;
            var openMilestones = await milestoneClient.GetAllForRepository(owner, repository, new MilestoneRequest { State = ItemStateFilter.Open }).ConfigureAwait(false);
            var milestone = openMilestones.FirstOrDefault(m => m.Title == milestoneTitle);

            if (milestone == null)
            {
                return;
            }

            await milestoneClient.Update(owner, repository, milestone.Number, new MilestoneUpdate { State = ItemState.Closed }).ConfigureAwait(false);

            if (_configuration.Close.IssueComments)
            {
                await AddIssueCommentsAsync(owner, repository, milestoneTitle).ConfigureAwait(false);
            }
        }

        public async Task OpenMilestone(string owner, string repository, string milestoneTitle)
        {
            var milestoneClient = _gitHubClient.Issue.Milestone;
            var closedMilestones = await milestoneClient.GetAllForRepository(owner, repository, new MilestoneRequest { State = ItemStateFilter.Closed }).ConfigureAwait(false);
            var milestone = closedMilestones.FirstOrDefault(m => m.Title == milestoneTitle);

            if (milestone == null)
            {
                return;
            }

            await milestoneClient.Update(owner, repository, milestone.Number, new MilestoneUpdate { State = ItemState.Open }).ConfigureAwait(false);
        }

        public async Task PublishRelease(string owner, string repository, string tagName)
        {
            var release = await GetReleaseFromTagNameAsync(owner, repository, tagName).ConfigureAwait(false);

            if (release == null)
            {
                return;
            }

            var releaseUpdate = new ReleaseUpdate { TagName = tagName, Draft = false };

            await _gitHubClient.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate).ConfigureAwait(false);
        }

        public async Task CreateLabels(string owner, string repository)
        {
            var newLabels = new List<NewLabel>
            {
                new NewLabel("Breaking change", "b60205"),
                new NewLabel("Bug", "ee0701"),
                new NewLabel("Build", "009800"),
                new NewLabel("Documentation", "d4c5f9"),
                new NewLabel("Feature", "84b6eb"),
                new NewLabel("Improvement", "207de5"),
                new NewLabel("Question", "cc317c"),
                new NewLabel("good first issue", "7057ff"),
                new NewLabel("help wanted", "33aa3f"),
            };

            var labels = await _gitHubClient.Issue.Labels.GetAllForRepository(owner, repository).ConfigureAwait(false);

            var deleteLabelsTasks = labels.Select(label => _gitHubClient.Issue.Labels.Delete(owner, repository, label.Name));
            await Task.WhenAll(deleteLabelsTasks).ConfigureAwait(false);

            var createLabelsTasks = newLabels.Select(label => _gitHubClient.Issue.Labels.Create(owner, repository, label));
            await Task.WhenAll(createLabelsTasks).ConfigureAwait(false);
        }

        private static NewRelease CreateNewRelease(string name, string tagName, string body, bool prerelease, string targetCommitish)
        {
            var newRelease = new NewRelease(tagName)
            {
                Draft = true,
                Body = body,
                Name = name,
                Prerelease = prerelease,
            };

            if (!string.IsNullOrEmpty(targetCommitish))
            {
                newRelease.TargetCommitish = targetCommitish;
            }

            return newRelease;
        }

        private static string ComputeSha256Hash(string asset)
        {
            // Create a SHA256
            using (var sha256Hash = SHA256.Create())
            {
                using (var fileStream = File.Open(asset, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // ComputeHash - returns byte array
                    var bytes = sha256Hash.ComputeHash(fileStream);

                    // Convert byte array to a string
                    var builder = new StringBuilder();

                    foreach (var t in bytes)
                    {
                        builder.Append(t.ToString("x2"));
                    }

                    return builder.ToString();
                }
            }
        }

        private async Task AddIssueCommentsAsync(string owner, string repository, string milestone)
        {
            const string detectionComment = "<!-- GitReleaseManager release comment -->";
            var issueComment = detectionComment + "\n" + _configuration.Close.IssueCommentFormat.ReplaceTemplate(new { owner, repository, Milestone = milestone });
            var issues = await GetIssuesFromMilestoneAsync(owner, repository, milestone).ConfigureAwait(false);

            foreach (var issue in issues)
            {
                if (issue.State != ItemState.Closed)
                {
                    continue;
                }

                SleepWhenRateIsLimited();

                try
                {
                    if (!await CommentsIncludeString(owner, repository, issue.Number, detectionComment).ConfigureAwait(false))
                    {
                        Logger.WriteInfo(string.Format("Adding release comment for issue #{0}", issue.Number));
                        await _gitHubClient.Issue.Comment.Create(owner, repository, issue.Number, issueComment).ConfigureAwait(false);
                }
                    else
                    {
                        Logger.WriteInfo(string.Format("Issue #{0} already contains release comment, skipping...", issue.Number));
                    }
                }
                catch (ForbiddenException)
                {
                    Logger.WriteWarning(string.Format("Unable to add comment to issue #{0}. Insufficient permissions.", issue.Number));
                    break;
                }
            }
        }

        private async Task<bool> CommentsIncludeString(string owner, string repository, int issueNumber, string comment)
        {
            var issueComments = await _gitHubClient.Issue.Comment.GetAllForIssue(owner, repository, issueNumber).ConfigureAwait(false);

            return issueComments.Any(c => c.Body.Contains(comment));
        }

        private Task<IReadOnlyList<Octokit.Issue>> GetIssuesFromMilestoneAsync(string owner, string repository, string milestone, ItemStateFilter state = ItemStateFilter.Closed)
        {
            return _gitHubClient.Issue.GetAllForRepository(owner, repository, new RepositoryIssueRequest
            {
                Milestone = milestone,
                State = state,
            });
        }

        private async Task<Octokit.Release> GetReleaseFromTagNameAsync(string owner, string repository, string tagName)
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(owner, repository).ConfigureAwait(false);

            var release = releases.FirstOrDefault(r => r.TagName == tagName);
            return release;
        }

        private void SleepWhenRateIsLimited()
        {
            var lastApi = _gitHubClient.GetLastApiInfo();
            if (lastApi?.RateLimit.Remaining == 0)
            {
                var sleepMs = lastApi.RateLimit.Reset.Millisecond - DateTimeOffset.Now.Millisecond;
                Logger.WriteWarning(string.Format("Rate limit exceeded, sleeping for {0} MS", sleepMs));
                Thread.Sleep(sleepMs);
            }
        }
    }
}