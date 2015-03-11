//-----------------------------------------------------------------------
// <copyright file="AddAssetSubOptions.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using CommandLine;

    public class AddAssetSubOptions : CommonSubOptions
    {
        [Option('a', "asset", HelpText = "Path to the file to include in the release.", Required = true)]
        public string AssetPath { get; set; }
    }
}