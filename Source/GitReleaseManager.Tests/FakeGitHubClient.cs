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

        public Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone)
        {
            return Task.FromResult(NumberOfCommits);
        }

        public Task<List<Issue>> GetIssues(Milestone targetMilestone)
        {
            return Task.FromResult(Issues);
        }

        public Task<List<Release>> GetReleases()
        {
            return Task.FromResult(Releases);
        }

        public Task<Release> GetSpecificRelease(string tagName)
        {
            return Task.FromResult(Release);
        }

        public ReadOnlyCollection<Milestone> GetMilestones()
        {
            return new ReadOnlyCollection<Milestone>(Milestones);
        }
    }
}