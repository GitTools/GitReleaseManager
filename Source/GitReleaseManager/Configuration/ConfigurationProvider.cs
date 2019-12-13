//-----------------------------------------------------------------------
// <copyright file="ConfigurationProvider.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using GitReleaseManager.Core.Helpers;

    public static class ConfigurationProvider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not required, as direct return of object")]
        public static Config Provide(string gitDirectory, IFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            var configFilePath = GetConfigFilePath(gitDirectory);

            if (fileSystem.Exists(configFilePath))
            {
                var readAllText = fileSystem.ReadAllText(configFilePath);
                using (var stringReader = new StringReader(readAllText))
                {
                    var deserializedConfig = ConfigSerializer.Read(stringReader);

                    EnsureDefaultConfig(deserializedConfig);

                    return deserializedConfig;
                }
            }

            return new Config();
        }

        public static string GetEffectiveConfigAsString(string currentDirectory, IFileSystem fileSystem)
        {
            var config = Provide(currentDirectory, fileSystem);
            var stringBuilder = new StringBuilder();
            using (var stream = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
            {
                ConfigSerializer.Write(config, stream);
                stream.Flush();
            }

            return stringBuilder.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Works as expected")]
        public static void WriteSample(string targetDirectory, IFileSystem fileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            var configFilePath = GetConfigFilePath(targetDirectory);

            if (!fileSystem.Exists(configFilePath))
            {
                using (var stream = fileSystem.OpenWrite(configFilePath))
                using (var writer = new StreamWriter(stream))
                {
                    ConfigSerializer.WriteSample(writer);
                }
            }
            else
            {
                Logger.WriteError("Cannot write sample, GitReleaseManager.yaml already exists");
            }
        }

        private static string GetConfigFilePath(string targetDirectory)
        {
            return Path.Combine(targetDirectory, "GitReleaseManager.yaml");
        }

        private static void EnsureDefaultConfig(Config configuration)
        {
            if (configuration.Create.ShaSectionHeading == null)
            {
                configuration.Create.ShaSectionHeading = "SHA256 Hashes of the release artifacts";
            }

            if (configuration.Create.ShaSectionLineFormat == null)
            {
                configuration.Create.ShaSectionLineFormat = "- `{1}\t{0}`";
            }

            if (configuration.Close.IssueCommentFormat == null)
            {
                configuration.Close.IssueCommentFormat = Config.IssueCommentFormat;
            }
        }
    }
}