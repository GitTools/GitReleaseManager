//-----------------------------------------------------------------------
// <copyright file="CreateSubOptions.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ReleaseNotesCompiler.CLI
{
    using CommandLine;

    public class CreateSubOptions : CommonSubOptions
    {
        [Option('a', "asset", HelpText = "Path to the file to include in the release.", Required = false)]
        public string AssetPath { get; set; }

        [Option('t', "targetcommitish", HelpText = "The commit to tag. Can be a branch or SHA. Defaults to repo's default branch.", Required = false)]
        public string TargetCommitish { get; set; }
    }
}