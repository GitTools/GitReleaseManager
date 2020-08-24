//-----------------------------------------------------------------------
// <copyright file="IReleaseNotesExporter.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using GitReleaseManager.Core.Model;

namespace GitReleaseManager.Core.ReleaseNotes
{
    public interface IReleaseNotesExporter
    {
        string ExportReleaseNotes(IEnumerable<Release> releases);
    }
}