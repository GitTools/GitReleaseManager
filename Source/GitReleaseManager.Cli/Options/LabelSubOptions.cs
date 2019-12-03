//-----------------------------------------------------------------------
// <copyright file="LabelSubOptions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Cli.Options
{
    using CommandLine;

    [Verb("label", HelpText = "Deletes existing labels and replaces with set of default labels.")]
    public class LabelSubOptions : BaseVcsOptions
    {
    }
}