//-----------------------------------------------------------------------
// <copyright file="FakeGitHubClient.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Model;
    using IVcsProvider = GitReleaseManager.Core.IVcsProvider;

    public class FakeGitHubClient : IVcsProvider
    {
        public FakeGitHubClient()
        {
            Milestones = new List<Milestone>();
            Issues = new List<Issue>();
            Releases = new List<Release>();
            Release = new Release();
        }

        public List<Milestone> Milestones { get; private set; }

        public List<Issue> Issues { get; private set; }

        public List<Release> Releases { get; private set; }

        public Release Release { get; private set; }

        public int NumberOfCommits { get; set; }

        public Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone, string user, string repository)
        {
            return Task.FromResult(NumberOfCommits);
        }

        public Task<List<Issue>> GetIssuesAsync(Milestone targetMilestone)
        {
            return Task.FromResult(Issues);
        }

        public Task<List<Release>> GetReleasesAsync(string user, string repository)
        {
            return Task.FromResult(Releases);
        }

        public Task<Release> GetSpecificRelease(string tagName, string user, string repository)
        {
            return Task.FromResult(Release);
        }

        public Task<ReadOnlyCollection<Milestone>> GetReadOnlyMilestonesAsync(string user, string repository)
        {
            return Task.FromResult(new ReadOnlyCollection<Milestone>(Milestones));
        }

        public string GetCommitsLink(string user, string repository, Milestone milestone, Milestone previousMilestone)
        {
            if (milestone is null)
            {
                throw new System.ArgumentNullException(nameof(milestone));
            }

            if (previousMilestone is null)
            {
                return string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/commits/{2}", user, repository, milestone.Title);
            }

            return string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/compare/{2}...{3}", user, repository, previousMilestone.Title, milestone.Title);
        }

        public Task<Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new System.NotImplementedException();
        }

        public Task<Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new System.NotImplementedException();
        }

        public Task DiscardRelease(string owner, string repository, string name)
        {
            throw new System.NotImplementedException();
        }

        public Task AddAssets(string owner, string repository, string tagName, IList<string> assets)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> ExportReleases(string owner, string repository, string tagName)
        {
            throw new System.NotImplementedException();
        }

        public Task CloseMilestoneAsync(string owner, string repository, string milestoneTitle)
        {
            throw new System.NotImplementedException();
        }

        public Task OpenMilestone(string owner, string repository, string milestoneTitle)
        {
            throw new System.NotImplementedException();
        }

        public Task PublishRelease(string owner, string repository, string tagName)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateLabelsAsync(string owner, string repository)
        {
            throw new System.NotImplementedException();
        }
    }
}