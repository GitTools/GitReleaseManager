//-----------------------------------------------------------------------
// <copyright file="CreateConfig.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using YamlDotNet.Serialization;

    public class CreateConfig
    {
        [YamlMember(Alias = "include-footer")]
        public bool IncludeFooter { get; set; }

        [YamlMember(Alias = "footer-heading")]
        public string FooterHeading { get; set; }

        [YamlMember(Alias = "footer-content")]
        public string FooterContent { get; set; }

        [YamlMember(Alias = "footer-includes-milestone")]
        public bool FooterIncludesMilestone { get; set; }

        [YamlMember(Alias = "milestone-replace-text")]
        public string MilestoneReplaceText { get; set; }

        [YamlMember(Alias = "include-sha-section")]
        public bool IncludeShaSection { get; set; }

        [YamlMember(Alias = "sha-section-heading")]
        public string ShaSectionHeading { get; set; }

        [YamlMember(Alias = "sha-section-line-format")]
        public string ShaSectionLineFormat { get; set; }
    }
}