//-----------------------------------------------------------------------
// <copyright file="GitHubProvider.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Octokit;
    using Milestone = Model.Milestone;
    using Release = Model.Release;
    using Issue = Model.Issue;
    using AutoMapper;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System;
    using GitReleaseManager.Core.Configuration;
    using System.Globalization;

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
                    var gitHubClientRepositoryCommitsCompare = await _gitHubClient.Repository.Commit.Compare(user, repository, "master", currentMilestone.Title);
                    return gitHubClientRepositoryCommitsCompare.AheadBy;
                }

                var compareResult = await _gitHubClient.Repository.Commit.Compare(user, repository, previousMilestone.Title, "master");
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
            var allIssues = await _gitHubClient.AllIssuesForMilestone(githubMilestone);
            return _mapper.Map<List<Issue>>(allIssues.Where(x => x.State == ItemState.Closed).ToList());
        }

        public async Task<List<Release>> GetReleases(string user, string repository)
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(user, repository);
            return _mapper.Map<List<Release>>(allReleases.OrderByDescending(r => r.CreatedAt).ToList());
        }

        public async Task<Release> GetSpecificRelease(string tagName, string user, string repository)
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(user, repository);
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
                    State = ItemStateFilter.Closed
                }).Result;

            var open = milestonesClient.GetAllForRepository(
                user,
                repository,
                new MilestoneRequest
                {
                    State = ItemStateFilter.Open
                }).Result;

            return new ReadOnlyCollection<Milestone>(_mapper.Map<List<Milestone>>(closed.Concat(open).ToList()));
        }

        private static NewRelease CreateNewRelease(string name, string tagName, string body, bool prerelease, string targetCommitish)
        {
            var newRelease = new NewRelease(tagName)
            {
                Draft = true,
                Body = body,
                Name = name,
                Prerelease = prerelease
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
            var releaseNotesBuilder = new ReleaseNotesBuilder(this, owner, repository, milestone, _configuration);

            var result = await releaseNotesBuilder.BuildReleaseNotes();

            var releaseUpdate = CreateNewRelease(releaseName, milestone, result, prerelease, targetCommitish);

            var release = await _gitHubClient.Repository.Release.Create(owner, repository, releaseUpdate);

            await AddAssets(owner, repository, milestone, assets);

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

            var release = await _gitHubClient.Repository.Release.Create(owner, repository, releaseUpdate);

            await AddAssets(owner, repository, name, assets);

            return _mapper.Map<Octokit.Release, Release>(release);
        }

        public async Task AddAssets(string owner, string repository, string tagName, IList<string> assets)
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(owner, repository);

            var release = releases.FirstOrDefault(r => r.TagName == tagName);

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
                        continue;
                    }

                    var upload = new ReleaseAssetUpload
                    {
                        FileName = Path.GetFileName(asset),
                        ContentType = "application/octet-stream",
                        RawData = File.Open(asset, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    };

                    await _gitHubClient.Repository.Release.UploadAsset(release, upload);

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
                    await _gitHubClient.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate);
                }
            }
        }

        public async Task<string> ExportReleases(string owner, string repository, string tagName)
        {
            var releaseNotesExporter = new ReleaseNotesExporter(this, _configuration, owner, repository);

            var result = await releaseNotesExporter.ExportReleaseNotes(tagName);

            return result;
        }

        public async Task CloseMilestone(string owner, string repository, string milestoneTitle)
        {
            var milestoneClient = _gitHubClient.Issue.Milestone;
            var openMilestones = await milestoneClient.GetAllForRepository(owner, repository, new MilestoneRequest { State = ItemStateFilter.Open });
            var milestone = openMilestones.FirstOrDefault(m => m.Title == milestoneTitle);

            if (milestone == null)
            {
                return;
            }

            await milestoneClient.Update(owner, repository, milestone.Number, new MilestoneUpdate { State = ItemState.Closed });

        }

        public async Task PublishRelease(string owner, string repository, string tagName)
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(owner, repository);
            var release = releases.FirstOrDefault(r => r.TagName == tagName);

            if (release == null)
            {
                return;
            }

            var releaseUpdate = new ReleaseUpdate { TagName = tagName, Draft = false };

            await _gitHubClient.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate);

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

            var labels = await _gitHubClient.Issue.Labels.GetAllForRepository(owner, repository);

            foreach (var label in labels)
            {
                await _gitHubClient.Issue.Labels.Delete(owner, repository, label.Name);
            }

            foreach (var label in newLabels)
            {
                await _gitHubClient.Issue.Labels.Create(owner, repository, label);
            }
        }
    }
}