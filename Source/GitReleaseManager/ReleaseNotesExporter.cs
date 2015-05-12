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
        private IGitHubClient gitHubClient;
        private Config configuration;

        public ReleaseNotesExporter(IGitHubClient gitHubClient, Config configuration)
        {
            this.gitHubClient = gitHubClient;
            this.configuration = configuration;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate.")]
        public async Task<string> ExportReleaseNotes(string tagName)
        {
            var stringBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(tagName))
            {
                var releases = await this.gitHubClient.GetReleases();

                if (releases.Count > 0)
                {
                    foreach (var release in releases)
                    {
                        this.AppendVersionReleaseNotes(stringBuilder, release);
                    }
                }
                else
                {
                    stringBuilder.Append("Unable to find any releases for specified repository.");
                }
            }
            else
            {
                var release = await this.gitHubClient.GetSpecificRelease(tagName);

                this.AppendVersionReleaseNotes(stringBuilder, release);
            }

            return stringBuilder.ToString();
        }

        private void AppendVersionReleaseNotes(StringBuilder stringBuilder, Release release)
        {
            if (this.configuration.Export.IncludeCreatedDateInTitle)
            {
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "## {0} ({1})", release.TagName, release.CreatedAt.ToString(this.configuration.Export.CreatedDateStringFormat, CultureInfo.InvariantCulture)));
            }
            else
            {
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "## {0}", release.TagName));
            }

            stringBuilder.AppendLine(Environment.NewLine);

            if (this.configuration.Export.PerformRegexRemoval)
            {
                var regexPattern = new Regex(this.configuration.Export.RegexText, this.configuration.Export.IsMultilineRegex ? RegexOptions.Multiline : RegexOptions.Singleline);
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