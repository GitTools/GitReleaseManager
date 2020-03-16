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
    using GitReleaseManager.Core.Exceptions;
    using GitReleaseManager.Core.Extensions;
    using Octokit;
    using Serilog;
    using Issue = GitReleaseManager.Core.Model.Issue;
    using Milestone = GitReleaseManager.Core.Model.Milestone;
    using Release = GitReleaseManager.Core.Model.Release;

    public class GitHubProvider : IVcsProvider
    {
        private readonly Config _configuration;
        private readonly ILogger _logger = Log.ForContext<GitHubProvider>();
        private readonly IMapper _mapper;
        private GitHubClient _gitHubClient;

        [Obsolete("Use overload with token only instead")]
        public GitHubProvider(IMapper mapper, Config configuration, string userName, string password, string token)
        {
            _mapper = mapper;
            _configuration = configuration;
            CreateClient(userName, password, token);
        }

        public GitHubProvider(IMapper mapper, Config configuration, string token)
        {
            _mapper = mapper;
            _configuration = configuration;
            CreateClient(token);
        }

        public Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone, string user, string repository)
        {
            if (currentMilestone is null)
            {
                throw new ArgumentNullException(nameof(currentMilestone));
            }

            return GetNumberOfCommitsBetweenInternal(previousMilestone, currentMilestone, user, repository);
        }

        public async Task<List<Issue>> GetIssuesAsync(Milestone targetMilestone)
        {
            var githubMilestone = _mapper.Map<Octokit.Milestone>(targetMilestone);
            _logger.Verbose("Finding issues on milestone: {@Milestone}", githubMilestone);
            var allIssues = await _gitHubClient.AllIssuesForMilestone(githubMilestone).ConfigureAwait(false);
            return _mapper.Map<List<Issue>>(allIssues.Where(x => x.State == ItemState.Closed).ToList());
        }

        public async Task<List<Release>> GetReleasesAsync(string user, string repository)
        {
            _logger.Verbose("Finding all releases on '{User}/{Repository}'", user, repository);
            var allReleases = await _gitHubClient.Repository.Release.GetAll(user, repository).ConfigureAwait(false);
            return _mapper.Map<List<Release>>(allReleases.OrderByDescending(r => r.CreatedAt).ToList());
        }

        public async Task<Release> GetSpecificRelease(string tagName, string user, string repository)
        {
            return _mapper.Map<Release>(await GetReleaseFromTagNameAsync(user, repository, tagName).ConfigureAwait(false));
        }

        public async Task<ReadOnlyCollection<Milestone>> GetReadOnlyMilestonesAsync(string user, string repository)
        {
            var milestonesClient = _gitHubClient.Issue.Milestone;

            _logger.Verbose("Finding all closed milestones on '{User}/{Repository}'", user, repository);
            var closed = await milestonesClient.GetAllForRepository(
                user,
                repository,
                new MilestoneRequest
                {
                    State = ItemStateFilter.Closed,
                }).ConfigureAwait(false);

            _logger.Verbose("Finding all open milestones on '{User}/{Repository}'", user, repository);
            var open = await milestonesClient.GetAllForRepository(
                user,
                repository,
                new MilestoneRequest
                {
                    State = ItemStateFilter.Open,
                }).ConfigureAwait(false);

            return new ReadOnlyCollection<Milestone>(_mapper.Map<List<Milestone>>(closed.Concat(open).ToList()));
        }

        [Obsolete("Use overload with token only instead")]
        public void CreateClient(string userName, string password, string token)
        {
            var credentials = string.IsNullOrWhiteSpace(token)
                ? new Credentials(userName, password)
                : new Credentials(token);

            var github = new GitHubClient(new ProductHeaderValue("GitReleaseManager")) { Credentials = credentials };
            _gitHubClient = github;
        }

        public void CreateClient(string token)
        {
            var credentials = new Credentials(token);
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
                _logger.Verbose("Creating new release on '{Owner}/{Repository}'", owner, repository);
                _logger.Debug("{@ReleaseUpdate}", releaseUpdate);
                release = await _gitHubClient.Repository.Release.Create(owner, repository, releaseUpdate).ConfigureAwait(false);
            }
            else
            {
                _logger.Warning("A release for milestone {Milestone} already exists, and will be updated", milestone);

                if (!release.Draft && !_configuration.Create.AllowUpdateToPublishedRelease)
                {
                    throw new InvalidOperationException("Release is not in draft state, so not updating.");
                }

                var releaseUpdate = release.ToUpdate();
                releaseUpdate.Body = result;
                _logger.Verbose("Updating release {Milestone} on '{Owner}/{Repository}'", milestone, owner, repository);
                _logger.Debug("{@ReleaseUpdate}", releaseUpdate);
                await _gitHubClient.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate).ConfigureAwait(false);
            }

            await AddAssets(owner, repository, milestone, assets).ConfigureAwait(false);
            return _mapper.Map<Octokit.Release, Release>(release);
        }

        public Task<Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new ArgumentException("Unable to locate input file.");
            }

            return CreateReleaseFromInputFileInternal(owner, repository, name, inputFilePath, targetCommitish, assets, prerelease);
        }

        public async Task DiscardRelease(string owner, string repository, string name)
        {
            var release = await GetReleaseFromTagNameAsync(owner, repository, name).ConfigureAwait(false);

            if (release is null)
            {
                throw new MissingReleaseException(string.Format("Unable to find a release with name {0}", name));
            }

            if (!release.Draft)
            {
                throw new InvalidStateException("Release is not in draft state, so not discarding.");
            }

            await _gitHubClient.Repository.Release.Delete(owner, repository, release.Id).ConfigureAwait(false);
        }

        public async Task AddAssets(string owner, string repository, string tagName, IList<string> assets)
        {
            var release = await GetReleaseFromTagNameAsync(owner, repository, tagName).ConfigureAwait(false);

            if (release is null)
            {
                _logger.Error("Unable to find Release with specified tagName");
                return;
            }

            if (!(assets is null))
            {
                foreach (var asset in assets)
                {
                    if (!File.Exists(asset))
                    {
                        var logMessage = string.Format("Requested asset to be uploaded doesn't exist: {0}", asset);
                        throw new FileNotFoundException(logMessage);
                    }

                    var assetFileName = Path.GetFileName(asset);

                    var existingAsset = release.Assets.FirstOrDefault(a => a.Name == assetFileName);
                    if (existingAsset != null)
                    {
                        _logger.Warning("Requested asset to be uploaded already exists on draft release, replacing with new file: {AssetPath}", asset);
                        await _gitHubClient.Repository.Release.DeleteAsset(owner, repository, existingAsset.Id).ConfigureAwait(false);
                    }

                    var upload = new ReleaseAssetUpload
                    {
                        FileName = assetFileName,
                        ContentType = "application/octet-stream",
                        RawData = File.Open(asset, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                    };

                    _logger.Verbose("Uploading asset '{FileName}' to release '{TagName}' on '{Owner}/{Repository}'", assetFileName, tagName, owner, repository);
                    _logger.Debug("{@Upload}", upload);

                    await _gitHubClient.Repository.Release.UploadAsset(release, upload).ConfigureAwait(false);

                    // Make sure to tidy up the stream that was created above
                    upload.RawData.Dispose();
                }

                if (assets.Any() && _configuration.Create.IncludeShaSection)
                {
                    var stringBuilder = new StringBuilder(release.Body);

                    if (!release.Body.Contains(_configuration.Create.ShaSectionHeading))
                    {
                        _logger.Debug("Creating SHA section header");
                        stringBuilder.AppendLine(string.Format("### {0}", _configuration.Create.ShaSectionHeading));
                    }

                    foreach (var asset in assets)
                    {
                        var file = new FileInfo(asset);

                        if (!file.Exists)
                        {
                            continue;
                        }

                        _logger.Debug("Creating SHA checksum for {Name}.", file.Name);

                        stringBuilder.AppendFormat(_configuration.Create.ShaSectionLineFormat, file.Name, ComputeSha256Hash(asset));
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.AppendLine();

                    var releaseUpdate = release.ToUpdate();
                    releaseUpdate.Body = stringBuilder.ToString();
                    _logger.Verbose("Updating release notes with sha checksum");
                    _logger.Debug("{@ReleaseUpdate}", releaseUpdate);
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
            _logger.Verbose("Finding open milestone with title '{Title}' on '{Owner}/{Repository}'", milestoneTitle, owner, repository);
            var milestoneClient = _gitHubClient.Issue.Milestone;
            var openMilestones = await milestoneClient.GetAllForRepository(owner, repository, new MilestoneRequest { State = ItemStateFilter.Open }).ConfigureAwait(false);
            var milestone = openMilestones.FirstOrDefault(m => m.Title == milestoneTitle);

            if (milestone is null)
            {
                _logger.Debug("No existing open milestone with title '{Title}' was found", milestoneTitle);
                return;
            }

            _logger.Verbose("Closing milestone '{Title}' on '{Owner}/{Repository}'", milestoneTitle, owner, repository);

            await milestoneClient.Update(owner, repository, milestone.Number, new MilestoneUpdate { State = ItemState.Closed }).ConfigureAwait(false);

            if (_configuration.Close.IssueComments)
            {
                await AddIssueCommentsAsync(owner, repository, milestone).ConfigureAwait(false);
            }
        }

        public async Task OpenMilestone(string owner, string repository, string milestoneTitle)
        {
            _logger.Verbose("Finding closed milestone with title '{Title}' on '{Owner}/{Repository}'", milestoneTitle, owner, repository);
            var milestoneClient = _gitHubClient.Issue.Milestone;
            var closedMilestones = await milestoneClient.GetAllForRepository(owner, repository, new MilestoneRequest { State = ItemStateFilter.Closed }).ConfigureAwait(false);
            var milestone = closedMilestones.FirstOrDefault(m => m.Title == milestoneTitle);

            if (milestone is null)
            {
                _logger.Debug("No existing closed milestone with title '{Title}' was found", milestoneTitle);
                return;
            }

            _logger.Verbose("Opening milestone '{Title}' on '{Owner}/{Repository}'", milestoneTitle, owner, repository);
            await milestoneClient.Update(owner, repository, milestone.Number, new MilestoneUpdate { State = ItemState.Open }).ConfigureAwait(false);
        }

        public async Task PublishRelease(string owner, string repository, string tagName)
        {
            var release = await GetReleaseFromTagNameAsync(owner, repository, tagName).ConfigureAwait(false);

            if (release is null)
            {
                _logger.Verbose("No release with tag '{TagName}' was found on '{Owner}/{Repository}'", tagName, owner, repository);
                return;
            }

            var releaseUpdate = new ReleaseUpdate { TagName = tagName, Draft = false };

            _logger.Verbose("Publishing release '{TagName}' on '{Owner}/{Repository}'", tagName, owner, repository);
            _logger.Debug("{@ReleaseUpdate}", releaseUpdate);
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

            _logger.Verbose("Grabbing all existing labels on '{Owner}/{Repository}'", owner, repository);
            var labels = await _gitHubClient.Issue.Labels.GetAllForRepository(owner, repository).ConfigureAwait(false);

            _logger.Verbose("Removing existing labels");
            _logger.Debug("{@Labels}", labels);
            var deleteLabelsTasks = labels.Select(label => _gitHubClient.Issue.Labels.Delete(owner, repository, label.Name));
            await Task.WhenAll(deleteLabelsTasks).ConfigureAwait(false);

            _logger.Verbose("Creating new standard labels");
            _logger.Debug("{@Labels}", newLabels);
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

        private async Task AddIssueCommentsAsync(string owner, string repository, Octokit.Milestone milestone)
        {
            const string detectionComment = "<!-- GitReleaseManager release comment -->";
            var issueComment = detectionComment + "\n" + _configuration.Close.IssueCommentFormat.ReplaceTemplate(new { owner, repository, Milestone = milestone.Title });
            var issues = await GetIssuesFromMilestoneAsync(owner, repository, milestone.Number).ConfigureAwait(false);

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
                        _logger.Information("Adding release comment for issue #{IssueNumber}", issue.Number);
                        await _gitHubClient.Issue.Comment.Create(owner, repository, issue.Number, issueComment).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.Information("Issue #{IssueNumber} already contains release comment, skipping...", issue.Number);
                    }
                }
                catch (ForbiddenException)
                {
                    _logger.Error("Unable to add comment to issue #{IssueNumber}. Insufficient permissions.", issue.Number);
                    break;
                }
            }
        }

        private async Task<bool> CommentsIncludeString(string owner, string repository, int issueNumber, string comment)
        {
            _logger.Verbose("Finding issue comment created by GitReleaseManager for issue #{IssueNumber}", issueNumber);
            var issueComments = await _gitHubClient.Issue.Comment.GetAllForIssue(owner, repository, issueNumber).ConfigureAwait(false);

            return issueComments.Any(c => c.Body.Contains(comment));
        }

        private async Task<Release> CreateReleaseFromInputFileInternal(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            _logger.Verbose("Reading release notes from: '{FilePath}'", inputFilePath);

            var inputFileContents = File.ReadAllText(inputFilePath);

            var releaseUpdate = CreateNewRelease(name, name, inputFileContents, prerelease, targetCommitish);

            _logger.Verbose("Creating new release on '{Owner}/{Repository}'", owner, repository);
            _logger.Debug("{@ReleaseUpdate}", releaseUpdate);

            var release = await _gitHubClient.Repository.Release.Create(owner, repository, releaseUpdate).ConfigureAwait(false);

            await AddAssets(owner, repository, name, assets).ConfigureAwait(false);

            return _mapper.Map<Octokit.Release, Release>(release);
        }

        private Task<IReadOnlyList<Octokit.Issue>> GetIssuesFromMilestoneAsync(string owner, string repository, int milestoneNumber, ItemStateFilter state = ItemStateFilter.Closed)
        {
            _logger.Verbose("Finding issues with milestone: '{Milestone}", milestoneNumber);
            return _gitHubClient.Issue.GetAllForRepository(owner, repository, new RepositoryIssueRequest
            {
                Milestone = milestoneNumber.ToString(),
                State = state,
            });
        }

        private async Task<int> GetNumberOfCommitsBetweenInternal(Milestone previousMilestone, Milestone currentMilestone, string user, string repository)
        {
            try
            {
                if (previousMilestone == null)
                {
                    _logger.Verbose("Getting commit count between base '{Base}' and head '{Head}'", "master", currentMilestone.Title);
                    var gitHubClientRepositoryCommitsCompare = await _gitHubClient.Repository.Commit.Compare(user, repository, "master", currentMilestone.Title).ConfigureAwait(false);
                    return gitHubClientRepositoryCommitsCompare.AheadBy;
                }

                _logger.Verbose("Getting commit count between base '{Base}' and head '{Head}'", previousMilestone.Title, "master");
                var compareResult = await _gitHubClient.Repository.Commit.Compare(user, repository, previousMilestone.Title, "master").ConfigureAwait(false);
                return compareResult.AheadBy;
            }
            catch (NotFoundException)
            {
                _logger.Warning("Unable to find tag for milestone, so commit count will be returned as zero");

                // If there is no tag yet the Compare will return a NotFoundException
                // we can safely ignore
                return 0;
            }
        }

        private async Task<Octokit.Release> GetReleaseFromTagNameAsync(string owner, string repository, string tagName)
        {
            _logger.Verbose("Finding release with tag name: '{TagName}'", tagName);
            var releases = await _gitHubClient.Repository.Release.GetAll(owner, repository).ConfigureAwait(false);

            var release = releases.FirstOrDefault(r => r.TagName == tagName);
            return release;
        }

        private void SleepWhenRateIsLimited()
        {
            var lastApi = _gitHubClient.GetLastApiInfo();
            if (lastApi?.RateLimit.Remaining == 0)
            {
                var sleepTime = lastApi.RateLimit.Reset - DateTimeOffset.Now;
                _logger.Warning("Rate limit exceeded, sleeping for {$SleepTime}", sleepTime);
                Thread.Sleep(sleepTime);
            }
        }
    }
}