using System.Globalization;

namespace GitReleaseManager.Core.Provider
{
    public class GitHubProvider : IVcsProvider
    {
        public string GetCommitsUrl(string owner, string repository, string baseMilestoneTitle, string compareMilestoneTitle = null)
        {
            Ensure.IsNotNullOrWhiteSpace(baseMilestoneTitle, nameof(baseMilestoneTitle));

            string url;

            if (string.IsNullOrWhiteSpace(compareMilestoneTitle))
            {
                url = string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/commits/{2}", owner, repository, baseMilestoneTitle);
            }
            else
            {
                url = string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/compare/{2}...{3}", owner, repository, compareMilestoneTitle, baseMilestoneTitle);
            }

            return url;
        }
    }
}