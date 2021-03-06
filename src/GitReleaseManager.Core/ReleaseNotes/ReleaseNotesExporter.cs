namespace GitReleaseManager.Core.ReleaseNotes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Model;
    using Serilog;

    public class ReleaseNotesExporter : IReleaseNotesExporter
    {
        private readonly ILogger _logger;
        private readonly ExportConfig _configuration;

        public ReleaseNotesExporter(ILogger logger, ExportConfig configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public string ExportReleaseNotes(IEnumerable<Release> releases)
        {
            _logger.Verbose("Exporting release notes");
            var stringBuilder = new StringBuilder();

            if (releases.Any())
            {
                foreach (var release in releases)
                {
                    AppendVersionReleaseNotes(stringBuilder, release);
                }

                _logger.Verbose("Finished exporting release notes");
            }
            else
            {
                stringBuilder.Append("Unable to find any releases for specified repository.");
            }

            return stringBuilder.ToString();
        }

        private void AppendVersionReleaseNotes(StringBuilder stringBuilder, Release release)
        {
            if (_configuration.IncludeCreatedDateInTitle)
            {
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "## {0} ({1})", release.TagName, release.CreatedAt.ToString(_configuration.CreatedDateStringFormat, CultureInfo.InvariantCulture)));
            }
            else
            {
                stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "## {0}", release.TagName));
            }

            stringBuilder.AppendLine(Environment.NewLine);

            if (_configuration.PerformRegexRemoval)
            {
                var regexPattern = new Regex(_configuration.RegexText, _configuration.IsMultilineRegex ? RegexOptions.Multiline : RegexOptions.Singleline);
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