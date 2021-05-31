using CommandLine;

namespace GitReleaseManager.Core.Options
{
    [Verb("label", HelpText = "Deletes existing labels and replaces with set of default labels.")]
    public class LabelSubOptions : BaseVcsOptions
    {
    }
}