using CommandLine;

namespace GitReleaseManager.Core.Options
{
    [Verb("publish", HelpText = "Publishes the Release.")]
    public class PublishSubOptions : BaseVcsOptions
    {
        [Option('t', "tagName", HelpText = "The name of the release (Typically this is the generated SemVer Version Number).", Required = true)]
        public string TagName { get; set; }
    }
}