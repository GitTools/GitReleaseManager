//-----------------------------------------------------------------------
// <copyright file="InitSubOptions.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli
{
    using CommandLine;
    using CommandLine.Text;

    public class InitSubOptions : BaseSubOptions
    {
        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}