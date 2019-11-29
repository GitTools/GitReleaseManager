//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesExporter.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Configuration;
    using Octokit;

    public class ReleaseNotesExporter
    {
        private IGitHubClient _gitHubClient;
        private Config _configuration;

        public ReleaseNotesExporter(IGitHubClient gitHubClient, Config configuration)
        {
            _gitHubClient = gitHubClient;
            _configuration = configuration;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate.")]
        public async Task<string> ExportReleaseNotes(string tagName)
        {
            var stringBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(tagName))
            {
                var releases = await _gitHubClient.GetReleases();

                if (releases.Count > 0)
                {
                    foreach (var release in releases)
                    {
                        AppendVersionReleaseNotes(stringBuilder, release);
                    }
                }
                else
                {
                    stringBuilder.Append("Unable to find any releases for specified repository.");
                }
            }
            else
            {
                var release = await _gitHubClient.GetSpecificRelease(tagName);

                AppendVersionReleaseNotes(stringBuilder, release);
            }

            return stringBuilder.ToString();
        }

        private void AppendVersionReleaseNotes(StringBuilder stringBuilder, Release release)
        {
            if (_configuration.Export.IncludeCreatedDateInTitle)
            {
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "## {0} ({1})", release.TagName, release.CreatedAt.ToString(_configuration.Export.CreatedDateStringFormat, CultureInfo.InvariantCulture)));
            }
            else
            {
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "## {0}", release.TagName));
            }

            stringBuilder.AppendLine(Environment.NewLine);

            if (_configuration.Export.PerformRegexRemoval)
            {
                var regexPattern = new Regex(_configuration.Export.RegexText, _configuration.Export.IsMultilineRegex ? RegexOptions.Multiline : RegexOptions.Singleline);
                var replacement = string.Empty;
                var replacedBody = regexPattern.Replace(release.Body, replacement);
                stringBuilder.AppendLine(replacedBody);
            }
            else
            {
                stringBuilder.AppendLine(release.Body);
            }
        }
    }
}