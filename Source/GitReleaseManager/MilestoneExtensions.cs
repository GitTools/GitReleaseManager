//-----------------------------------------------------------------------
// <copyright file="MilestoneExtensions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using System;
    using Octokit;

    public static class MilestoneExtensions
    {
        public static Version Version(this Milestone ver)
        {
            var nameWithoutPrerelease = ver.Title.Split('-')[0];
            if (nameWithoutPrerelease.StartsWith("v"))
            {
                nameWithoutPrerelease = nameWithoutPrerelease.Remove(0, 1);
            }

            Version parsedVersion;

            if (!System.Version.TryParse(nameWithoutPrerelease, out parsedVersion))
            {
                return new Version(0, 0);
            }

            return parsedVersion;
        }
    }
}