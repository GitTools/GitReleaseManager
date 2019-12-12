//-----------------------------------------------------------------------
// <copyright file="InitSubOptions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Cli.Options
{
    using CommandLine;

    [Verb("init", HelpText = "Creates a sample Yaml Configuration file in root directory")]
    public class InitSubOptions : BaseSubOptions
    {
    }
}