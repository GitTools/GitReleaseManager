//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesExporter.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using GitHubReleaseManager.Configuration;

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
        public async Task<string> ExportReleaseNotes()
        {
            var releases = await this.gitHubClient.GetReleases();

            var stringBuilder = new StringBuilder();

            if (releases.Count > 0)
            {
                foreach (var release in releases)
                {
                    if (this.configuration.Export.IncludeCreatedDateInTitle)
                    {
                        stringBuilder.AppendLine(string.Format("## {0} ({1})", release.TagName, release.CreatedAt.ToString(this.configuration.Export.CreatedDateStringFormat)));
                    }
                    else
                    {
                        stringBuilder.AppendLine(string.Format("## {0}", release.TagName));
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
            else
            {
                stringBuilder.Append("Unable to find any releases for specified repository.");
            }

            return stringBuilder.ToString();
        }
    }
}