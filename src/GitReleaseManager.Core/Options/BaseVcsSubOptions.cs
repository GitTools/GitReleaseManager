using CommandLine;
using Destructurama.Attributed;
using GitReleaseManager.Core.Model;

namespace GitReleaseManager.Core.Options
{
    public abstract class BaseVcsOptions : BaseSubOptions
    {
        [LogMasked(Text = "[REDACTED]")]
        [Option("token", HelpText = "The Access Token to access Version Control System with.", Required = true, SetName = "OAuth flow")]
        public string Token { get; set; }

        [Option('o', "owner", HelpText = "The owner of the repository.", Required = true)]
        public string RepositoryOwner { get; set; }

        [Option('r', "repository", HelpText = "The name of the repository.", Required = true)]
        public string RepositoryName { get; set; }

        [Option("provider", HelpText = "Version Control System provider", Default = VcsProvider.GitHub)]
        public VcsProvider Provider { get; set; }
    }
}