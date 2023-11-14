using System.Threading.Tasks;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Options;
using Serilog;

namespace GitReleaseManager.Core.Commands
{
    public class ShowConfigCommand : ICommand<ShowConfigSubOptions>
    {
        private readonly ILogger _logger;
        private readonly Config _config;

        public ShowConfigCommand(ILogger logger, Config config)
        {
            _logger = logger;
            _config = config;
        }

        public Task<int> ExecuteAsync(ShowConfigSubOptions options)
        {
            var configuration = ConfigurationProvider.GetEffectiveConfigAsString(_config);
            _logger.Information("{Configuration}", configuration);

            return Task.FromResult(0);
        }
    }
}