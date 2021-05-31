using System.Threading.Tasks;
using GitReleaseManager.Core.Options;
using Serilog;

namespace GitReleaseManager.Core.Commands
{
    public class PublishCommand : ICommand<PublishSubOptions>
    {
        private readonly IVcsService _vcsService;
        private readonly ILogger _logger;

        public PublishCommand(IVcsService vcsService, ILogger logger)
        {
            _vcsService = vcsService;
            _logger = logger;
        }

        public async Task<int> Execute(PublishSubOptions options)
        {
            _logger.Information("Publish release {TagName}", options.TagName);
            await _vcsService.PublishReleaseAsync(options.RepositoryOwner, options.RepositoryName, options.TagName).ConfigureAwait(false);

            return 0;
        }
    }
}