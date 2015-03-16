//-----------------------------------------------------------------------
// <copyright file="CommonSubOptions.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using CommandLine;

    public abstract class CommonSubOptions : BaseGitHubSubOptions
    {
        [Option('m', "milestone", HelpText = "The milestone to use.", Required = true)]
        public string Milestone { get; set; }
    }
}