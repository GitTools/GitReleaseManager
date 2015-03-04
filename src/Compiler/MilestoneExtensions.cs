using System;

namespace ReleaseNotesCompiler
{
    using Octokit;

    static class MilestoneExtensions
    {
        internal static Version Version(this Milestone ver)
        {
            var nameWithoutPrerelease = ver.Title.Split('-')[0];
            Version parsedVersion;

            if (!System.Version.TryParse(nameWithoutPrerelease, out parsedVersion))
            {
                return new Version(0, 0);
            }

            return parsedVersion;
        }
    }
}