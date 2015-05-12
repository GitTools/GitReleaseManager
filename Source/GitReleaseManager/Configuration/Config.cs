//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="gep13">
//     Copyright (c) 2015 - Present gep13
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using System.Collections.Generic;
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
                                  MilestoneReplaceText = string.Empty
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