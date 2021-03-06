namespace GitReleaseManager.Core.ReleaseNotes
{
    using System.Collections.Generic;
    using GitReleaseManager.Core.Model;

    public interface IReleaseNotesExporter
    {
        string ExportReleaseNotes(IEnumerable<Release> releases);
    }
}