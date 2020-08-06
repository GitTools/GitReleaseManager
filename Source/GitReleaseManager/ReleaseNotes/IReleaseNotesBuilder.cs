using System.Threading.Tasks;

namespace GitReleaseManager.Core.ReleaseNotes
{
    public interface IReleaseNotesBuilder
    {
        Task<string> BuildReleaseNotes(string user, string repository, string milestoneTitle);
    }
}