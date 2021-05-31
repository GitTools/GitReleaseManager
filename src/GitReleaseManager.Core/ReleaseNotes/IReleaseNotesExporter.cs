using System.Collections.Generic;
using GitReleaseManager.Core.Model;

namespace GitReleaseManager.Core.ReleaseNotes
{
    public interface IReleaseNotesExporter
    {
        string ExportReleaseNotes(IEnumerable<Release> releases);
    }
}