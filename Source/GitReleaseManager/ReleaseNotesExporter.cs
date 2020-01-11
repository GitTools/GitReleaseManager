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
    using GitReleaseManager.Core.Model;
    using Serilog;

    public class ReleaseNotesExporter
    {
        private readonly ILogger _logger = Log.ForContext<ReleaseNotesExporter>();
        private readonly IVcsProvider _vcsProvider;
        private readonly Config _configuration;
        private readonly string _user;
        private readonly string _repository;

        public ReleaseNotesExporter(IVcsProvider vcsProvider, Config configuration, string user, string repository)
        {
            _vcsProvider = vcsProvider;
            _configuration = configuration;
            _user = user;
            _repository = repository;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate.")]
        public async Task<string> ExportReleaseNotes(string tagName)
        {
            _logger.Verbose("Exporting release notes");
            var stringBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(tagName))
            {
                var releases = await _vcsProvider.GetReleasesAsync(_user, _repository).ConfigureAwait(false);

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
                var release = await _vcsProvider.GetSpecificRelease(tagName, _user, _repository).ConfigureAwait(false);

                AppendVersionReleaseNotes(stringBuilder, release);
            }

            _logger.Verbose("Finished exporting release notes");

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