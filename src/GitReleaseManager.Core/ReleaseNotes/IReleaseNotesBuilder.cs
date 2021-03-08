namespace GitReleaseManager.Core.ReleaseNotes
{
    using System.Threading.Tasks;

    public interface IReleaseNotesBuilder
    {
        Task<string> BuildReleaseNotes(string user, string repository, string milestoneTitle, string template);
    }
}