//-----------------------------------------------------------------------
// <copyright file="BaseSubOptions.cs" company="gep13">
//     Copyright (c) 2015 - Present gep13
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using CommandLine;

    public abstract class BaseSubOptions
    {
        [Option('t', "targetPath", HelpText = "The directory on which GitHubReleaseManager should be executed. Defaults to current directory.", Required = false)]
        public string TargetPath { get; set; }

        [Option('l', "logFilePath", HelpText = "Path to where log file should be created. Defaults to logging to console.", Required = false)]
        public string LogFilePath { get; set; }
    }
}