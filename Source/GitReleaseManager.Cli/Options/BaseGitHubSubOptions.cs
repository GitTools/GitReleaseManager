// <copyright file="BaseGitHubSubOptions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Cli.Options
{
    using CommandLine;
    using Octokit;

    public abstract class BaseGitHubSubOptions : BaseSubOptions
    {
        [Option('u', "username", HelpText = "The username to access GitHub with.", Required = true, SetName = "Basic Auth")]
        public string UserName { get; set; }

        [Option('p', "password", HelpText = "The password to access GitHub with.", Required = true, SetName = "Basic Auth")]
        public string Password { get; set; }

        [Option("token", HelpText = "The Access Token to access GitHub with.", Required = true, SetName = "OAuth flow")]
        public string Token { get; set; }

        [Option('o', "owner", HelpText = "The owner of the repository.", Required = true)]
        public string RepositoryOwner { get; set; }

        [Option('r', "repository", HelpText = "The name of the repository.", Required = true)]
        public string RepositoryName { get; set; }

        public GitHubClient CreateGitHubClient()
        {
            var credentials = string.IsNullOrWhiteSpace(Token)
                ? new Credentials(UserName, Password)
                : new Credentials(Token);

            var github = new GitHubClient(new ProductHeaderValue("GitReleaseManager")) { Credentials = credentials };
            return github;
        }
    }
}