//-----------------------------------------------------------------------
// <copyright file="VcsServiceMock.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Model;
    using IVcsService = GitReleaseManager.Core.IVcsService;

    public class VcsServiceMock : IVcsService
    {
        public VcsServiceMock()
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

        public Task<Release> CreateReleaseFromMilestoneAsync(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new System.NotImplementedException();
        }

        public Task<Release> CreateReleaseFromInputFileAsync(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new System.NotImplementedException();
        }

        public Task DiscardReleaseAsync(string owner, string repository, string tagName)
        {
            throw new System.NotImplementedException();
        }

        public Task AddAssetsAsync(string owner, string repository, string tagName, IList<string> assets)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> ExportReleasesAsync(string owner, string repository, string tagName)
        {
            throw new System.NotImplementedException();
        }

        public Task CloseMilestoneAsync(string owner, string repository, string milestoneTitle)
        {
            throw new System.NotImplementedException();
        }

        public Task OpenMilestoneAsync(string owner, string repository, string milestoneTitle)
        {
            throw new System.NotImplementedException();
        }

        public Task PublishReleaseAsync(string owner, string repository, string tagName)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateLabelsAsync(string owner, string repository)
        {
            throw new System.NotImplementedException();
        }
    }
}