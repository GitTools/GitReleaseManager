using System;
using System.Globalization;
using System.IO;
using System.Text;
using GitReleaseManager.Core.Helpers;
using Serilog;

namespace GitReleaseManager.Core.Configuration
{
    public static class ConfigurationProvider
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(ConfigurationProvider));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not required, as direct return of object")]
        public static Config Provide(string gitDirectory, IFileSystem fileSystem)
        {
            if (fileSystem is null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            var configFilePath = GetConfigFilePath(gitDirectory);

            if (configFilePath != null)
            {
                _logger.Verbose("Loading configuration from file: {FilePath}", configFilePath);

                var readAllText = fileSystem.ReadAllText(configFilePath);
                using (var stringReader = new StringReader(readAllText))
                {
                    var deserializedConfig = ConfigSerializer.Read(stringReader);

                    EnsureDefaultConfig(deserializedConfig);

                    return deserializedConfig;
                }
            }

            _logger.Warning("Yaml not found, that's ok! Learn more at {Url}", "https://gittools.github.io/GitReleaseManager/docs/yaml");

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

            var defaultConfigFilePath = Path.Combine(targetDirectory, "GitReleaseManager.yml");

            if (!fileSystem.Exists(defaultConfigFilePath))
            {
                _logger.Information("Writing sample file to '{ConfigFilePath}'", defaultConfigFilePath);

                // The following try/finally statements is to ensure that
                // any stream is not disposed more than once.
                Stream stream = null;
                try
                {
                    stream = fileSystem.OpenWrite(defaultConfigFilePath);
                    using (var writer = new StreamWriter(stream))
                    {
                        stream = null;
                        ConfigSerializer.WriteSample(writer);
                    }
                }
                finally
                {
                    stream?.Dispose();
                }
            }
            else
            {
                _logger.Error("Cannot write sample, '{File}' already exists", defaultConfigFilePath);
            }
        }

        private static string GetConfigFilePath(string targetDirectory)
        {
            var filePath = Path.Combine(targetDirectory, "GitReleaseManager.yaml");
            if (File.Exists(filePath))
            {
                return filePath;
            }

            filePath = Path.Combine(targetDirectory, "GitReleaseManager.yml");
            if (File.Exists(filePath))
            {
                return filePath;
            }

            return null;
        }

        private static void EnsureDefaultConfig(Config configuration)
        {
            if (configuration.Create.ShaSectionHeading is null)
            {
                _logger.Debug("Setting default Create.ShaSectionHeading configuration value");
                configuration.Create.ShaSectionHeading = "SHA256 Hashes of the release artifacts";
            }

            if (configuration.Create.ShaSectionLineFormat == null)
            {
                _logger.Debug("Setting default Create.ShaSectionLineFormat configuration value");
                configuration.Create.ShaSectionLineFormat = "- `{1}\t{0}`";
            }

            if (configuration.Close.IssueCommentFormat == null)
            {
                _logger.Debug("Setting default Close.IssueCommentFormat configuration value");
                configuration.Close.IssueCommentFormat = Config.ISSUE_COMMENT_FORMAT;
            }
        }
    }
}