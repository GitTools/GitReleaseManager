//-----------------------------------------------------------------------
// <copyright file="MainOptions.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using CommandLine;
    using CommandLine.Text;

    public class MainOptions
    {
        [VerbOption("create", HelpText = "Creates a draft release notes from a milestone.")]
        public CreateSubOptions CreateVerb { get; set; }

        [VerbOption("addasset", HelpText = "Adds an asset to an existing release.")]
        public AddAssetSubOptions AddAssetVerb { get; set; }

        [VerbOption("close", HelpText = "Closes the milestone.")]
        public CloseSubOptions CloseVerb { get; set; }

        [VerbOption("publish", HelpText = "Publishes the release notes and closes the milestone.")]
        public PublishSubOptions PublishVerb { get; set; }

        [VerbOption("export", HelpText = "Exports all the Release Notes in markdown format.")]
        public ExportSubOptions ExportVerb { get; set; }

        [VerbOption("init", HelpText = "Creates a sample Yaml Configuration file in root directory")]
        public InitSubOptions InitVerb { get; set; }

        [VerbOption("showconfig", HelpText = "Shows the current configuration")]
        public ShowConfigSubOptions ShowConfigVerb { get; set; }

        [HelpVerbOption]
        public string DoHelpForVerb(string verbName)
        {
            return HelpText.AutoBuild(this, verbName);
        }
    }
}