namespace GitReleaseManager.Core.Commands
{
    using System;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using GitReleaseManager.Core.Options;
    using Serilog;

    public class ShowConfigCommand : ICommand<ShowConfigSubOptions>
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public ShowConfigCommand(IFileSystem fileSystem, ILogger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public Task<int> Execute(ShowConfigSubOptions options)
        {
            var configuration = ConfigurationProvider.GetEffectiveConfigAsString(options.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);
            _logger.Information("{Configuration}", configuration);

            return Task.FromResult(0);
        }
    }
}