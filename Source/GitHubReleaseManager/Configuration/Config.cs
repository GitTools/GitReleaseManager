//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Configuration
{
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    public class Config
    {
        public Config()
        {
            this.Create = new CreateConfig
                              {
                                  IncludeFooter = true,
                                  FooterHeading = "Where to get it",
                                  FooterContent = "You can download this release from [chocolatey](https://chocolatey.org/packages/ChocolateyGUI/{milestone})",
                                  FooterIncludesMilestone = true,
                                  MilestoneReplaceText = "{milestone}"
                              };

            this.Export = new ExportConfig
                              {
                                  IncludeCreatedDateInTitle = true,
                                  CreatedDateStringFormat = "MMMM dd, yyyy",
                                  PerformRegexRemoval = true,
                                  RegexText = @"### Where to get it(\r\n)*You can .*\)",
                                  IsMultilineRegex = true
                              };

            this.IssueLabelsInclude = new List<string>
                                   {
                                       "Bug",
                                       "Feature",
                                       "Improvement"
                                   };

            this.IssueLabelsExclude = new List<string>
                                   {
                                       "Internal Refactoring"
                                   };
        }

        [YamlMember(Alias = "create")]
        public CreateConfig Create { get; private set; }

        [YamlMember(Alias = "export")]
        public ExportConfig Export { get; private set; }

        [YamlMember(Alias = "issue-labels-include")]
        public IList<string> IssueLabelsInclude { get; private set; }

        [YamlMember(Alias = "issue-labels-exclude")]
        public IList<string> IssueLabelsExclude { get; private set; }
    }
}