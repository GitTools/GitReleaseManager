using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Octokit;
using Issue = GitReleaseManager.Core.Model.Issue;
using ItemStateFilter = GitReleaseManager.Core.Model.ItemStateFilter;
using Milestone = GitReleaseManager.Core.Model.Milestone;
using NotFoundException = GitReleaseManager.Core.Exceptions.NotFoundException;
using Release = GitReleaseManager.Core.Model.Release;

namespace GitReleaseManager.Core.Provider
{
    public class GitHubProvider : IVcsProvider
    {
        private readonly IGitHubClient _gitHubClient;
        private readonly IMapper _mapper;

        public GitHubProvider(IGitHubClient gitHubClient, IMapper mapper)
        {
            _gitHubClient = gitHubClient;
            _mapper = mapper;
        }

        public async Task<int> GetCommitsCount(string owner, string repository, string @base, string head)
        {
            try
            {
                var result = await _gitHubClient.Repository.Commit.Compare(owner, repository, @base, head).ConfigureAwait(false);
                return result.AheadBy;
            }
            catch (Octokit.NotFoundException)
            {
                // If there is no tag yet the Compare will return a NotFoundException
                // we can safely ignore
                return 0;
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message, ex);
            }
        }

        public string GetCommitsUrl(string owner, string repository, string head, string @base = null)
        {
            Ensure.IsNotNullOrWhiteSpace(owner, nameof(owner));
            Ensure.IsNotNullOrWhiteSpace(repository, nameof(repository));
            Ensure.IsNotNullOrWhiteSpace(head, nameof(head));

            string url;

            if (string.IsNullOrWhiteSpace(@base))
            {
                url = string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/commits/{2}", owner, repository, head);
            }
            else
            {
                url = string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/compare/{2}...{3}", owner, repository, @base, head);
            }

            return url;
        }

        public async Task<IEnumerable<Issue>> GetIssuesAsync(string owner, string repository, int milestoneNumber, ItemStateFilter itemStateFilter = ItemStateFilter.All)
        {
            try
            {
                var openIssueRequest = new RepositoryIssueRequest
                {
                    Milestone = milestoneNumber.ToString(CultureInfo.InvariantCulture),
                    State = (Octokit.ItemStateFilter)itemStateFilter,
                };

                var issues = await _gitHubClient.Issue.GetAllForRepository(owner, repository, openIssueRequest).ConfigureAwait(false);

                return _mapper.Map<IEnumerable<Issue>>(issues);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message, ex);
            }
        }

        public async Task<IEnumerable<Milestone>> GetMilestonesAsync(string owner, string repository, ItemStateFilter itemStateFilter = ItemStateFilter.All)
        {
            try
            {
                var request = new MilestoneRequest { State = (Octokit.ItemStateFilter)itemStateFilter };
                var milestones = await _gitHubClient.Issue.Milestone.GetAllForRepository(owner, repository, request).ConfigureAwait(false);

                return _mapper.Map<IEnumerable<Milestone>>(milestones);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message, ex);
            }
        }

        public async Task<Release> GetReleaseAsync(string owner, string repository, string tagName)
        {
            try
            {
                var release = await _gitHubClient.Repository.Release.Get(owner, repository, tagName).ConfigureAwait(false);

                return _mapper.Map<Release>(release);
            }
            catch (Octokit.NotFoundException ex)
            {
                throw new NotFoundException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message, ex);
            }
        }

        public async Task<IEnumerable<Release>> GetReleasesAsync(string owner, string repository)
        {
            try
            {
                var releases = await _gitHubClient.Repository.Release.GetAll(owner, repository).ConfigureAwait(false);
                releases = releases.OrderByDescending(r => r.CreatedAt).ToList();

                return _mapper.Map<IEnumerable<Release>>(releases);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message, ex);
            }
        }
    }
}