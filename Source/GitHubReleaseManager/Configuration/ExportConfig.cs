//-----------------------------------------------------------------------
// <copyright file="ExportConfig.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Configuration
{
    using YamlDotNet.Serialization;

    public class ExportConfig
    {
        [YamlMember(Alias = "include-created-date-in-title")]
        public bool IncludeCreatedDateInTitle { get; set; }

        [YamlMember(Alias = "created-date-string-format")]
        public string CreatedDateStringFormat { get; set; }

        [YamlMember(Alias = "perform-regex-removal")]
        public bool PerformRegexRemoval { get; set; }

        [YamlMember(Alias = "regex-text")]
        public string RegexText { get; set; }

        [YamlMember(Alias = "multiline-regex")]
        public bool IsMultilineRegex { get; set; }
    }
}