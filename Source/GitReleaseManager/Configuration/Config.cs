//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using YamlDotNet.Serialization;

    public class Config
    {
        public Config()
        {
            this.Create = new CreateConfig
                              {
                                  IncludeFooter = false,
                                  FooterHeading = string.Empty,
                                  FooterContent = string.Empty,
                                  FooterIncludesMilestone = false,
                                  MilestoneReplaceText = string.Empty,
                                  IncludeShaSection = false,
                                  ShaSectionHeading = "SHA256 Hashes of the release artifacts",
                                  ShaSectionLineFormat = "- `{1}\t{0}`"
            };

            this.Export = new ExportConfig
                              {
                                  IncludeCreatedDateInTitle = false,
                                  CreatedDateStringFormat = string.Empty,
                                  PerformRegexRemoval = false,
                                  RegexText = string.Empty,
                                  IsMultilineRegex = false
                              };

            this.IssueLabelsInclude = new List<string>
                                   {
                                       "Bug",
                                       "Duplicate",
                                       "Enhancement",
                                       "Feature",
                                       "Help Wanted",
                                       "Improvement",
                                       "Invalid",
                                       "Question",
                                       "WontFix"
                                   };

            this.IssueLabelsExclude = new List<string>
                                   {
                                       "Internal Refactoring"
                                   };

            this.LabelAliases = new List<LabelAlias>();
        }

        [Description("Configuration values used when creating new releases")]
        [YamlMember(Alias = "create")]
        public CreateConfig Create { get; private set; }

        [Description("Configuration values used when exporting release notes")]
        [YamlMember(Alias = "export")]
        public ExportConfig Export { get; private set; }

        [Description("The labels that will be used to include issues in release notes.")]
        [YamlMember(Alias = "issue-labels-include")]
        public IList<string> IssueLabelsInclude { get; private set; }

        [Description("The labels that will NOT be used when including issues in release notes.")]
        [YamlMember(Alias = "issue-labels-exclude")]
        public IList<string> IssueLabelsExclude { get; private set; }

        [Description("Overrides default pluralization and header names for specific labels.")]
        [YamlMember(Alias = "issue-labels-alias")]
        public IList<LabelAlias> LabelAliases { get; private set; }
    }
}
