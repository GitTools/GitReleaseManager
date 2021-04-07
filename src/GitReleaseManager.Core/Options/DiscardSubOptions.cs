using CommandLine;

namespace GitReleaseManager.Core.Options
{
    [Verb("discard", HelpText = "Discards a draft release.")]
    public class DiscardSubOptions : BaseVcsOptions
    {
        [Option('m', "milestone", HelpText = "The milestone to use.", Required = true)]
        public string Milestone { get; set; }
    }
}