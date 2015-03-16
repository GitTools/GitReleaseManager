//-----------------------------------------------------------------------
// <copyright file="CreateSubOptions.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using CommandLine;

    public class CreateSubOptions : CommonSubOptions
    {
        [Option('a', "asset", HelpText = "Path to the file to include in the release.", Required = false)]
        public string AssetPath { get; set; }

        [Option('t', "targetcommitish", HelpText = "The commit to tag. Can be a branch or SHA. Defaults to repository's default branch.", Required = false)]
        public string TargetCommitish { get; set; }
    }
}