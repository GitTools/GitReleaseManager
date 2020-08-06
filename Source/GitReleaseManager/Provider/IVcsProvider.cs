using System.Collections.Generic;
using System.Threading.Tasks;
using GitReleaseManager.Core.Model;

namespace GitReleaseManager.Core.Provider
{
    public interface IVcsProvider
    {
        Task<int> GetCommitsCount(string owner, string repository, string @base, string head);

        string GetCommitsUrl(string owner, string repository, string head, string @base = null);

        Task<IEnumerable<Issue>> GetIssuesAsync(string owner, string repository, int milestoneNumber, ItemStateFilter itemStateFilter = ItemStateFilter.All);

        Task<IEnumerable<Milestone>> GetMilestonesAsync(string owner, string repository, ItemStateFilter itemStateFilter = ItemStateFilter.All);
    }
}