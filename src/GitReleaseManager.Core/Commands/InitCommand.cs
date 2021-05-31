using System;
using System.IO;
using System.Threading.Tasks;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Helpers;
using GitReleaseManager.Core.Options;
using GitReleaseManager.Core.Templates;
using Serilog;

namespace GitReleaseManager.Core.Commands
{
    public class InitCommand : ICommand<InitSubOptions>
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public InitCommand(IFileSystem fileSystem, ILogger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public Task<int> Execute(InitSubOptions options)
        {
            var directory = options.TargetDirectory ?? Environment.CurrentDirectory;

            if (!options.ExtractTemplates)
            {
                _logger.Information("Creating sample configuration file");
                ConfigurationProvider.WriteSample(directory, _fileSystem);
            }
            else
            {
                _logger.Information("Extracting Embedded templates");

                var templates = ReleaseTemplates.GetExportTemplates();
                var config = ConfigurationProvider.Provide(directory, _fileSystem);
                var templatesDirectory = new DirectoryInfo(_fileSystem.ResolvePath(config.TemplatesDirectory));

                if (!templatesDirectory.Exists)
                {
                    templatesDirectory.Create();
                }

                foreach (var template in templates)
                {
                    var fullFilePath = Path.Combine(templatesDirectory.FullName, template.Key);
                    var templateDir = Path.GetDirectoryName(fullFilePath);
                    if (!Directory.Exists(templateDir))
                    {
                        Directory.CreateDirectory(templateDir);
                    }
                    else if (File.Exists(fullFilePath))
                    {
                        _logger.Warning("File '{FilePath}' already exist. Skipping file.", template.Key);
                        continue;
                    }

                    _logger.Information("Creating new file '{FilePath}'!", template.Key);
                    _fileSystem.WriteAllText(fullFilePath, template.Value);
                }

                _logger.Information("All embedded templates has been extracted!");
            }

            return Task.FromResult(0);
        }
    }
}