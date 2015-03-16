//-----------------------------------------------------------------------
// <copyright file="DefaultGitHubClient.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Octokit;

    public class DefaultGitHubClient : IGitHubClient
    {
        private GitHubClient gitHubClient;
        private string user;
        private string repository;

        public DefaultGitHubClient(GitHubClient gitHubClient, string user, string repository)
        {
            this.gitHubClient = gitHubClient;
            this.user = user;
            this.repository = repository;
        }

        public async Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone)
        {
            try
            {
                if (previousMilestone == null)
                {
                    var gitHubClientRepositoryCommitsCompare = await this.gitHubClient.Repository.Commits.Compare(this.user, this.repository, "master", currentMilestone.Title);
                    return gitHubClientRepositoryCommitsCompare.AheadBy;
                }

                var compareResult = await this.gitHubClient.Repository.Commits.Compare(this.user, this.repository, previousMilestone.Title, "master");
                return compareResult.AheadBy;
            }
            catch (NotFoundException)
            {
                // If there is not tag yet the Compare will return a NotFoundException
                // we can safely ignore
                return 0;
            }
        }

        public async Task<List<Issue>> GetIssues(Milestone targetMilestone)
        {
            var allIssues = await this.gitHubClient.AllIssuesForMilestone(targetMilestone);
            return allIssues.Where(x => x.State == ItemState.Closed).ToList();
        }

        public async Task<List<Release>> GetReleases()
        {
            var allReleases = await this.gitHubClient.Release.GetAll(this.user, this.repository);
            return allReleases.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public ReadOnlyCollection<Milestone> GetMilestones()
        {
            var milestonesClient = this.gitHubClient.Issue.Milestone;
            var closed = milestonesClient.GetForRepository(
                this.user, 
                this.repository, 
                new MilestoneRequest
                    {
                        State = ItemState.Closed
                    }).Result;

            var open = milestonesClient.GetForRepository(
                this.user, 
                this.repository, 
                new MilestoneRequest
                    {
                        State = ItemState.Open
                    }).Result;

            return new ReadOnlyCollection<Milestone>(closed.Concat(open).ToList());
        }
    }
}