using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AutoMapper;
using Octokit;
using Issue = GitReleaseManager.Core.Model.Issue;
using ItemStateFilter = GitReleaseManager.Core.Model.ItemStateFilter;

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

        public string GetCommitsUrl(string owner, string repository, string milestoneTitle, string compareMilestoneTitle = null)
        {
            Ensure.IsNotNullOrWhiteSpace(owner, nameof(owner));
            Ensure.IsNotNullOrWhiteSpace(repository, nameof(repository));
            Ensure.IsNotNullOrWhiteSpace(milestoneTitle, nameof(milestoneTitle));

            string url;

            if (string.IsNullOrWhiteSpace(compareMilestoneTitle))
            {
                url = string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/commits/{2}", owner, repository, milestoneTitle);
            }
            else
            {
                url = string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/compare/{2}...{3}", owner, repository, compareMilestoneTitle, milestoneTitle);
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
            catch (ApiValidationException ex)
            {
                throw new ApiException(ex.Message, ex);
            }
        }
    }
}