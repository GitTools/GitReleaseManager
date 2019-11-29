//-----------------------------------------------------------------------
// <copyright file="DefaultGitHubClient.cs" company="GitTools Contributors">
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

    public class DefaultGitHubClient : IVcsClient
    {
        private GitHubClient _gitHubClient;
        private string _user;
        private string _repository;

        public DefaultGitHubClient(GitHubClient gitHubClient, string user, string repository)
        {
            _gitHubClient = gitHubClient;
            _user = user;
            _repository = repository;
        }

        public async Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone)
        {
            try
            {
                if (previousMilestone == null)
                {
                    var gitHubClientRepositoryCommitsCompare = await _gitHubClient.Repository.Commit.Compare(_user, _repository, "master", currentMilestone.Title);
                    return gitHubClientRepositoryCommitsCompare.AheadBy;
                }

                var compareResult = await _gitHubClient.Repository.Commit.Compare(_user, _repository, previousMilestone.Title, "master");
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
            var allIssues = await _gitHubClient.AllIssuesForMilestone(targetMilestone);
            return allIssues.Where(x => x.State == ItemState.Closed).ToList();
        }

        public async Task<List<Release>> GetReleases()
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(_user, _repository);
            return allReleases.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public async Task<Release> GetSpecificRelease(string tagName)
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(_user, _repository);
            return allReleases.FirstOrDefault(r => r.TagName == tagName);
        }

        public ReadOnlyCollection<Milestone> GetMilestones()
        {
            var milestonesClient = _gitHubClient.Issue.Milestone;
            var closed = milestonesClient.GetAllForRepository(
                _user,
                _repository,
                new MilestoneRequest
                {
                    State = ItemStateFilter.Closed
                }).Result;

            var open = milestonesClient.GetAllForRepository(
                _user,
                _repository,
                new MilestoneRequest
                {
                    State = ItemStateFilter.Open
                }).Result;

            return new ReadOnlyCollection<Milestone>(closed.Concat(open).ToList());
        }
    }
}