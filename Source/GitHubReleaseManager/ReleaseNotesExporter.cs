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

    public class ReleaseNotesExporter
    {
        private IGitHubClient gitHubClient;

        public ReleaseNotesExporter(IGitHubClient gitHubClient)
        {
            this.gitHubClient = gitHubClient;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate.")]
        public async Task<string> GetReleases()
        {
            var releases = await this.gitHubClient.GetReleases();

            var stringBuilder = new StringBuilder();

            if (releases.Count > 0)
            {
                foreach (var release in releases)
                {
                    stringBuilder.AppendLine(string.Format("## {0} ({1})", release.TagName, release.CreatedAt.ToString("MMMM dd, yyyy")));
                    stringBuilder.AppendLine(Environment.NewLine);

                    var regexPattern = new Regex(@"### Where to get it(\r\n)*You can .*\)", RegexOptions.Multiline);
                    var replacement = string.Empty;
                    var replacedBody = regexPattern.Replace(release.Body, replacement);
                    stringBuilder.AppendLine(replacedBody);
                }
            }

            return stringBuilder.ToString();
        }
    }
}