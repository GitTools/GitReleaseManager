//-----------------------------------------------------------------------
// <copyright file="CommonSubOptions.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli
{
    using CommandLine;

    public abstract class CommonSubOptions : BaseGitHubSubConfig
    {
        [Option('m', "milestone", HelpText = "The milestone to use.", Required = true)]
        public string Milestone { get; set; }
    }
}