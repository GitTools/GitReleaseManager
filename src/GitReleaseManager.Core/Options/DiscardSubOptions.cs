//-----------------------------------------------------------------------
// <copyright file="DiscardSubOptions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Options
{
    using CommandLine;

    [Verb("discard", HelpText = "Discards a draft release.")]
    public class DiscardSubOptions : BaseVcsOptions
    {
        [Option('m', "milestone", HelpText = "The milestone to use.", Required = true)]
        public string Milestone { get; set; }
    }
}
