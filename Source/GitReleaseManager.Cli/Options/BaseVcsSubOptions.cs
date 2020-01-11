// <copyright file="BaseVcsOptions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Cli.Options
{
    using CommandLine;
    using Destructurama.Attributed;

    public abstract class BaseVcsOptions : BaseSubOptions
    {
        [Option('u', "username", HelpText = "The username to access Version Control System with.", Required = true, SetName = "Basic Auth")]
        public string UserName { get; set; }

        [LogMasked(Text = "[REDACTED]")]
        [Option('p', "password", HelpText = "The password to access Version Control System with.", Required = true, SetName = "Basic Auth")]
        public string Password { get; set; }

        [LogMasked(Text = "[REDACTED]")]
        [Option("token", HelpText = "The Access Token to access Version Control System with.", Required = true, SetName = "OAuth flow")]
        public string Token { get; set; }

        [Option('o', "owner", HelpText = "The owner of the repository.", Required = true)]
        public string RepositoryOwner { get; set; }

        [Option('r', "repository", HelpText = "The name of the repository.", Required = true)]
        public string RepositoryName { get; set; }
    }
}