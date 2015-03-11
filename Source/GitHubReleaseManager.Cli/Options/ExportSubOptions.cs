//-----------------------------------------------------------------------
// <copyright file="ExportSubOptions.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using CommandLine;

    public class ExportSubOptions : BaseGitHubSubOptions
    {
        [Option('f', "fileOutputPath", HelpText = "Path to the file export releases.", Required = true)]
        public string FileOutputPath { get; set; }
    }
}