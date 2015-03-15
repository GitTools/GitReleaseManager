//-----------------------------------------------------------------------
// <copyright file="MilestoneExtensions.cs" company="gep13">
//     Copyright (c) 2015 - Present gep13
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager
{
    using System;
    using Octokit;

    public static class MilestoneExtensions
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