namespace GitReleaseManager.Core.Options
{
    using CommandLine;

    [Verb("init", HelpText = "Creates a sample Yaml Configuration file in root directory")]
    public class InitSubOptions : BaseSubOptions
    {
        [Option("templates", HelpText = "Extract the embedded templates to allow modification", Required = false)]
        public bool ExtractTemplates { get; set; }
    }
}