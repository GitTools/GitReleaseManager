//-----------------------------------------------------------------------
// <copyright file="DiscardOptions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Cli.Options
{
    using CommandLine;

    [Verb("discard", HelpText = "Discards a draft release.")]
    public class DiscardSubOptions : BaseVcsOptions
    {
        [Option('m', "milestone", HelpText = "The milestone to use.", Required = false)]
        public string Milestone { get; set; }
    }
}
