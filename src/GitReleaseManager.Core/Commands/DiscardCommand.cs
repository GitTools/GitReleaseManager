using System.Threading.Tasks;
using GitReleaseManager.Core.Options;
using Serilog;

namespace GitReleaseManager.Core.Commands
{
    public class DiscardCommand : ICommand<DiscardSubOptions>
    {
        private readonly IVcsService _vcsService;
        private readonly ILogger _logger;

        public DiscardCommand(IVcsService vcsService, ILogger logger)
        {
            _vcsService = vcsService;
            _logger = logger;
        }

        public async Task<int> ExecuteAsync(DiscardSubOptions options)
        {
            _logger.Information("Discarding release {Milestone}", options.Milestone);
            await _vcsService.DiscardReleaseAsync(options.RepositoryOwner, options.RepositoryName, options.Milestone).ConfigureAwait(false);

            return 0;
        }
    }
}