//-----------------------------------------------------------------------
// <copyright file="CreateSubOptions.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using System.Collections.Generic;

    using CommandLine;

    public class CreateSubOptions : CommonSubOptions
    {
        [OptionList('a', "assets", Separator = ',', HelpText = "Paths to the files to include in the release.", Required = false)]
        public IList<string> AssetPaths { get; set; }

        [Option('c', "targetcommitish", HelpText = "The commit to tag. Can be a branch or SHA. Defaults to repository's default branch.", Required = false)]
        public string TargetCommitish { get; set; }
    }
}