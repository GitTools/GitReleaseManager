//-----------------------------------------------------------------------
// <copyright file="CreateSubOptions.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using System.Collections.Generic;

    using CommandLine;

    public class CreateSubOptions : BaseGitHubSubOptions
    {
        [OptionList('a', "assets", Separator = ',', HelpText = "Paths to the files to include in the release.", Required = false)]
        public IList<string> AssetPaths { get; set; }

        [Option('c', "targetcommitish", HelpText = "The commit to tag. Can be a branch or SHA. Defaults to repository's default branch.", Required = false)]
        public string TargetCommitish { get; set; }

        [Option('m', "milestone", HelpText = "The milestone to use.", Required = false)]
        public string Milestone { get; set; }

        [Option('n', "name", HelpText = "The name of the release (Typically this is the generated SemVer Version Number.", Required = false)]
        public string Name { get; set; }

        [Option('i', "inputFilePath", HelpText = "The path to the file to be used as the content of the release notes.", Required = false)]
        public string InputFilePath { get; set; }
    }
}