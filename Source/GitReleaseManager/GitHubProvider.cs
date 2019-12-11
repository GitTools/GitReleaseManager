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
    using System.Threading.Tasks;
    using AutoMapper;
    using GitReleaseManager.Core.Configuration;
    using Octokit;
    using Issue = GitReleaseManager.Core.Model.Issue;
    using Milestone = GitReleaseManager.Core.Model.Milestone;
    using Release = GitReleaseManager.Core.Model.Release;

    public class GitHubProvider : IVcsProvider
    {
        private GitHubClient _gitHubClient;
        private IMapper _mapper;
        private Config _configuration;

        public GitHubProvider(IMapper mapper, Config configuration, string userName, string password, string token)
        {
            _mapper = mapper;
            _configuration = configuration;
            CreateClient(userName, password, token);
        }

        public async Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone, string user, string repository)
        {
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

        public async Task<List<Issue>> GetIssues(Milestone targetMilestone)
        {
            var githubMilestone = _mapper.Map<Octokit.Milestone>(targetMilestone);
            var allIssues = await _gitHubClient.AllIssuesForMilestone(githubMilestone).ConfigureAwait(false);
            return _mapper.Map<List<Issue>>(allIssues.Where(x => x.State == ItemState.Closed).ToList());
        }

        public async Task<List<Release>> GetReleases(string user, string repository)
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(user, repository).ConfigureAwait(false);
            return _mapper.Map<List<Release>>(allReleases.OrderByDescending(r => r.CreatedAt).ToList());
        }

        public async Task<Release> GetSpecificRelease(string tagName, string user, string repository)
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(user, repository).ConfigureAwait(false);
            return _mapper.Map<Release>(allReleases.FirstOrDefault(r => r.TagName == tagName));
        }

        public ReadOnlyCollection<Milestone> GetMilestones(string user, string repository)
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
            if (previousMilestone == null)
            {
                return string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/commits/{2}", user, repository, milestone.Title);
            }

            return string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/compare/{2}...{3}", user, repository, previousMilestone.Title, milestone.Title);
        }

        public async Task<Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease)
        {
            var release = await GetRelease(owner, repository, milestone).ConfigureAwait(false);
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
            var allReleases = await _gitHubClient.Repository.Release.GetAll(owner, repository);
            var release = allReleases.FirstOrDefault(r => r.TagName == name);

            if (release == null)
            {
                throw new Exception(string.Format("Unable to find a release with name {0}", name));
            }

            if (!release.Draft)
            {
                throw new Exception("Release is not in draft state, so not discarding.");
            }

            await _gitHubClient.Repository.Release.Delete(owner, repository, release.Id);
            return;
        }

        public async Task AddAssets(string owner, string repository, string tagName, IList<string> assets)
        {
            var release = await GetRelease(owner, repository, tagName).ConfigureAwait(false);

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
        }

        public async Task PublishRelease(string owner, string repository, string tagName)
        {
            var release = await GetRelease(owner, repository, tagName).ConfigureAwait(false);

            if (release == null)
            {
                return;
            }

            var releaseUpdate = new ReleaseUpdate { TagName = tagName, Draft = false };

            await _gitHubClient.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate).ConfigureAwait(false);
        }

        public async Task CreateLabels(string owner, string repository)
        {
            var newLabels = new List<NewLabel>();
            newLabels.Add(new NewLabel("Breaking change", "b60205"));
            newLabels.Add(new NewLabel("Bug", "ee0701"));
            newLabels.Add(new NewLabel("Build", "009800"));
            newLabels.Add(new NewLabel("Documentation", "d4c5f9"));
            newLabels.Add(new NewLabel("Feature", "84b6eb"));
            newLabels.Add(new NewLabel("Improvement", "207de5"));
            newLabels.Add(new NewLabel("Question", "cc317c"));
            newLabels.Add(new NewLabel("good first issue", "7057ff"));
            newLabels.Add(new NewLabel("help wanted", "33aa3f"));

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

        private async Task<Octokit.Release> GetRelease(string owner, string repository, string tagName)
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(owner, repository).ConfigureAwait(false);
            var release = releases.FirstOrDefault(r => r.TagName == tagName);
            return release;
        }
    }
}