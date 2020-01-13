//-----------------------------------------------------------------------
// <copyright file="ExportConfig.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using GitReleaseManager.Core.Attributes;
    using YamlDotNet.Serialization;

    public class ExportConfig
    {
        [YamlMember(Alias = "include-created-date-in-title")]
        public bool IncludeCreatedDateInTitle { get; set; }

        [Sample("MMMM dd, yyyy")]
        [YamlMember(Alias = "created-date-string-format")]
        public string CreatedDateStringFormat { get; set; }

        [YamlMember(Alias = "perform-regex-removal")]
        public bool PerformRegexRemoval { get; set; }

        [Sample("### Where to get it(\\r\\n)*You can .*\\.")]
        [YamlMember(Alias = "regex-text")]
        public string RegexText { get; set; }

        [YamlMember(Alias = "multiline-regex")]
        public bool IsMultilineRegex { get; set; }
    }
}