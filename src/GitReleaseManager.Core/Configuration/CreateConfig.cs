//-----------------------------------------------------------------------
// <copyright file="CreateConfig.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using System.ComponentModel;
    using GitReleaseManager.Core.Attributes;
    using YamlDotNet.Serialization;

    public class CreateConfig
    {
        [Description("Enable generation of footer content in the release notes. Extract the recommended templates by running 'init --templates' and edit the footer.sbn file to provide the wanted footer content.")]
        [Sample(true)]
        [YamlMember(Alias = "include-footer")]
        public bool IncludeFooter { get; set; }

        [YamlMember(Alias = "footer-heading", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string FooterHeading { get; set; }

        [YamlMember(Alias = "footer-content", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string FooterContent { get; set; }

        [YamlMember(Alias = "footer-includes-milestone", DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public bool FooterIncludesMilestone { get; set; }

        [YamlMember(Alias = "milestone-replace-text", DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string MilestoneReplaceText { get; set; }

        [YamlMember(Alias = "include-sha-section")]
        public bool IncludeShaSection { get; set; }

        [YamlMember(Alias = "sha-section-heading")]
        public string ShaSectionHeading { get; set; }

        [YamlMember(Alias = "sha-section-line-format")]
        public string ShaSectionLineFormat { get; set; }

        [YamlMember(Alias = "allow-update-to-published")]
        public bool AllowUpdateToPublishedRelease { get; set; }
    }
}