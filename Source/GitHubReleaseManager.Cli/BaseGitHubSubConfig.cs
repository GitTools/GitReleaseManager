// <copyright file="BaseGitHubSubConfig.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli
{
    using CommandLine;
    using Octokit;

    public abstract class BaseGitHubSubConfig : BaseSubOptions
    {
        [Option('u', "username", HelpText = "The username to access GitHub with.", Required = true)]
        public string UserName { get; set; }

        [Option('p', "password", HelpText = "The password to access GitHub with.", Required = true)]
        public string Password { get; set; }

        [Option('o', "owner", HelpText = "The owner of the repository.", Required = true)]
        public string RepositoryOwner { get; set; }

        [Option('r', "repository", HelpText = "The name of the repository.", Required = true)]
        public string RepositoryName { get; set; }

        public GitHubClient CreateGitHubClient()
        {
            var creds = new Credentials(this.UserName, this.Password);
            var github = new GitHubClient(new ProductHeaderValue("GitHubReleaseManager")) { Credentials = creds };
            return github;
        }
    }
}