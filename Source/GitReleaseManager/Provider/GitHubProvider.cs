using System.Globalization;

namespace GitReleaseManager.Core.Provider
{
    public class GitHubProvider : IVcsProvider
    {
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
    }
}