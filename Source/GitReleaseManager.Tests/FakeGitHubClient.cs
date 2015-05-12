//-----------------------------------------------------------------------
// <copyright file="FakeGitHubClient.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Octokit;
    using IGitHubClient = GitReleaseManager.Core.IGitHubClient;

    public class FakeGitHubClient : IGitHubClient
    {
        public FakeGitHubClient()
        {
            this.Milestones = new List<Milestone>();
            this.Issues = new List<Issue>();
            this.Releases = new List<Release>();
            this.Release = new Release();
        }

        public List<Milestone> Milestones { get; private set; }

        public List<Issue> Issues { get; private set; }

        public List<Release> Releases { get; private set; }

        public Release Release { get; private set; }

        public int NumberOfCommits { private get; set; }

        public Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone)
        {
            return Task.FromResult(this.NumberOfCommits);
        }

        public Task<List<Issue>> GetIssues(Milestone targetMilestone)
        {
            return Task.FromResult(this.Issues);
        }

        public Task<List<Release>> GetReleases()
        {
            return Task.FromResult(this.Releases);
        }

        public Task<Release> GetSpecificRelease(string tagName)
        {
            return Task.FromResult(this.Release);
        }

        public ReadOnlyCollection<Milestone> GetMilestones()
        {
            return new ReadOnlyCollection<Milestone>(this.Milestones);
        }
    }
}