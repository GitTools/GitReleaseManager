namespace GitReleaseManager.Core.Provider
{
    public interface IVcsProvider
    {
        string GetCommitsUrl(string owner, string repository, string baseMilestoneTitle, string compareMilestoneTitle = null);
    }
}