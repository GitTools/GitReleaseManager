//-----------------------------------------------------------------------
// <copyright file="FakeGitHubClient.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Tests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Octokit;
    using IGitHubClient = GitHubReleaseManager.IGitHubClient;

    public class FakeGitHubClient : IGitHubClient
    {
        public FakeGitHubClient()
        {
            this.Milestones = new List<Milestone>();
            this.Issues = new List<Issue>();
        }

        public List<Milestone> Milestones { get; set; }

        public List<Issue> Issues { get; set; }

        public int NumberOfCommits { get; set; }

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
            throw new System.NotImplementedException();
        }

        public ReadOnlyCollection<Milestone> GetMilestones()
        {
            return new ReadOnlyCollection<Milestone>(this.Milestones);
        }
    }
}