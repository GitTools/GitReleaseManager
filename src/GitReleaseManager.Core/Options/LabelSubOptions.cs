namespace GitReleaseManager.Core.Options
{
    using CommandLine;

    [Verb("label", HelpText = "Deletes existing labels and replaces with set of default labels.")]
    public class LabelSubOptions : BaseVcsOptions
    {
    }
}