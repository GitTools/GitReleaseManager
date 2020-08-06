using System.Collections.Generic;
using System.Threading.Tasks;
using GitReleaseManager.Core.Model;

namespace GitReleaseManager.Core.Provider
{
    public interface IVcsProvider
    {
        string GetCommitsUrl(string owner, string repository, string milestoneTitle, string compareMilestoneTitle = null);

        Task<IEnumerable<Issue>> GetIssuesAsync(string owner, string repository, int milestoneNumber, ItemStateFilter itemStateFilter = ItemStateFilter.All);
    }
}