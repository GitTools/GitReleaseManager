//-----------------------------------------------------------------------
// <copyright file="IReleaseNotesExporter.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.ReleaseNotes
{
    using System.Collections.Generic;
    using GitReleaseManager.Core.Model;

    public interface IReleaseNotesExporter
    {
        string ExportReleaseNotes(IEnumerable<Release> releases);
    }
}