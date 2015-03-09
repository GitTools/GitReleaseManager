//-----------------------------------------------------------------------
// <copyright file="Config.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Configuration
{
    using YamlDotNet.Serialization;

    public class Config
    {
        public Config()
        {
            this.ExportRegex = @"### Where to get it(\r\n)*You can .*\)";
        }

        [YamlMember(Alias = "export-regex")]
        public string ExportRegex { get; set; }
    }
}