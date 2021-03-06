namespace GitReleaseManager.Core.Options
{
    using CommandLine;

    [Verb("open", HelpText = "Opens the milestone.")]
    public class OpenSubOptions : BaseVcsOptions
    {
        [Option('m', "milestone", HelpText = "The milestone to use.", Required = true)]
        public string Milestone { get; set; }
    }
}