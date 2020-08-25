//-----------------------------------------------------------------------
// <copyright file="MilestoneExtensions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Extensions
{
    using System;
    using Octokit;
    using Serilog;

    public static class MilestoneExtensions
    {
        public static readonly ILogger _logger = Log.ForContext(typeof(MilestoneExtensions));

        public static Version Version(this Milestone ver)
        {
            if (ver is null)
            {
                throw new ArgumentNullException(nameof(ver));
            }

            var nameWithoutPrerelease = ver.Title.Split('-')[0];
            if (nameWithoutPrerelease.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug("Removing version prefix from {Name}", ver.Title);
                nameWithoutPrerelease = nameWithoutPrerelease.Remove(0, 1);
            }

            if (!System.Version.TryParse(nameWithoutPrerelease, out Version parsedVersion))
            {
                _logger.Warning("No valid version was found on {Title}", ver.Title);
                return new Version(0, 0);
            }

            return parsedVersion;
        }
    }
}