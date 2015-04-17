//-----------------------------------------------------------------------
// <copyright file="AddAssetSubOptions.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli.Options
{
    using System.Collections.Generic;

    using CommandLine;

    public class AddAssetSubOptions : BaseGitHubSubOptions
    {
        [OptionList('a', "assets", Separator = ',', HelpText = "Paths to the files to include in the release.", Required = true)]
        public IList<string> AssetPaths { get; set; }

        [Option('t', "tagName", HelpText = "The name of the release (Typically this is the generated SemVer Version Number).", Required = true)]
        public string TagName { get; set; }
    }
}