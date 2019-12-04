//-----------------------------------------------------------------------
// <copyright file="FakeGitHubClient.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
    using GitReleaseManager.Core.Model;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using IVcsClient = Core.IVcsClient;

    public class FakeGitHubClient : IVcsClient
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

        public int NumberOfCommits { private get; set; }

        public Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone, string user, string repository)
        {
            return Task.FromResult(NumberOfCommits);
        }

        public Task<List<Issue>> GetIssues(Milestone targetMilestone)
        {
            return Task.FromResult(Issues);
        }

        public Task<List<Release>> GetReleases(string user, string repository)
        {
            return Task.FromResult(Releases);
        }

        public Task<Release> GetSpecificRelease(string tagName, string user, string repository)
        {
            return Task.FromResult(Release);
        }

        public ReadOnlyCollection<Milestone> GetMilestones(string user, string repository)
        {
            return new ReadOnlyCollection<Milestone>(Milestones);
        }

        public string GetCommitsLink(string user, string repository, Milestone milestone, Milestone previousMilestone)
        {
            throw new System.NotImplementedException();
        }

        public Task<Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new System.NotImplementedException();
        }

        public Task<Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
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

        public Task CloseMilestone(string owner, string repository, string milestoneTitle)
        {
            throw new System.NotImplementedException();
        }

        public Task PublishRelease(string owner, string repository, string tagName)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateLabels(string owner, string repository)
        {
            throw new System.NotImplementedException();
        }
    }
}