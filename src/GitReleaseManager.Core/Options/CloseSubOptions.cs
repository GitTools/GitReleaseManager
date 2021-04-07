using CommandLine;

namespace GitReleaseManager.Core.Options
{
    [Verb("close", HelpText = "Closes the milestone.")]
    public class CloseSubOptions : BaseVcsOptions
    {
        [Option('m', "milestone", HelpText = "The milestone to use.", Required = true)]
        public string Milestone { get; set; }
    }
}