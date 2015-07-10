//-----------------------------------------------------------------------
// <copyright file="ExportConfig.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
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