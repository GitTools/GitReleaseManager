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
            this.ExportRegex = @"### Where to get it(\r\n)*You can .*\)";
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

        [YamlMember(Alias = "export-regex")]
        public string ExportRegex { get; set; }

        [YamlMember(Alias = "issue-labels-include")]
        public IList<string> IssueLabelsInclude { get; private set; }

        [YamlMember(Alias = "issue-labels-exclude")]
        public IList<string> IssueLabelsExclude { get; private set; }
    }
}