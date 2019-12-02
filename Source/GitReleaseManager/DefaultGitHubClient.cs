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
    using AutoMapper;

    public class DefaultGitHubClient : IVcsClient
    {
        private GitHubClient _gitHubClient;
        private string _user;
        private string _repository;
        private IMapper _mapper;

        public DefaultGitHubClient(GitHubClient gitHubClient, string user, string repository, IMapper mapper)
        {
            _gitHubClient = gitHubClient;
            _user = user;
            _repository = repository;
            _mapper = mapper;
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
            var githubMilestone = _mapper.Map<Octokit.Milestone>(targetMilestone);
            var allIssues = await _gitHubClient.AllIssuesForMilestone(githubMilestone);
            return _mapper.Map<List<Issue>>(allIssues.Where(x => x.State == ItemState.Closed).ToList());
        }

        public async Task<List<Release>> GetReleases()
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(_user, _repository);
            return _mapper.Map<List<Release>>(allReleases.OrderByDescending(r => r.CreatedAt).ToList());
        }

        public async Task<Release> GetSpecificRelease(string tagName)
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(_user, _repository);
            return _mapper.Map<Release>(allReleases.FirstOrDefault(r => r.TagName == tagName));
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

            return new ReadOnlyCollection<Milestone>(_mapper.Map<List<Milestone>>(closed.Concat(open).ToList()));
        }
    }
}