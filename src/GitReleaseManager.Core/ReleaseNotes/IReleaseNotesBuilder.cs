using System.Threading.Tasks;

namespace GitReleaseManager.Core.ReleaseNotes
{
    public interface IReleaseNotesBuilder
    {
        Task<string> BuildReleaseNotesAsync(string user, string repository, string milestoneTitle, string template);
    }
}