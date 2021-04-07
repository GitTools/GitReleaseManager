using System.Collections.Generic;
using CommandLine;

namespace GitReleaseManager.Core.Options
{
    [Verb("create", HelpText = "Creates a draft release notes from a milestone.")]
    public class CreateSubOptions : BaseVcsOptions
    {
        [Option('a', "assets", Separator = ',', HelpText = "Paths to the files to include in the release.", Required = false)]
        public IList<string> AssetPaths { get; set; }

        [Option('c', "targetcommitish", HelpText = "The commit to tag. Can be a branch or SHA. Defaults to repository's default branch.", Required = false)]
        public string TargetCommitish { get; set; }

        [Option('m', "milestone", HelpText = "The milestone to use.", Required = false)]
        public string Milestone { get; set; }

        [Option('n', "name", HelpText = "The name of the release (Typically this is the generated SemVer Version Number).", Required = false)]
        public string Name { get; set; }

        [Option('i', "inputFilePath", HelpText = "The path to the file to be used as the content of the release notes.", Required = false)]
        public string InputFilePath { get; set; }

        [Option('t', "template", HelpText = "The name of the template file to use. Can also be a relative or absolute path (relative paths are resolved from yaml template-dir configuration). Defaults to 'default'")]
        public string Template { get; set; }

        [Option('e', "pre", Required = false, HelpText = "Creates the release as a pre-release.")]
        public bool Prerelease { get; set; }
    }
}