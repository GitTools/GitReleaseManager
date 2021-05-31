using System.Threading.Tasks;
using GitReleaseManager.Core.Options;
using Serilog;

namespace GitReleaseManager.Core.Commands
{
    public class OpenCommand : ICommand<OpenSubOptions>
    {
        private readonly IVcsService _vcsService;
        private readonly ILogger _logger;

        public OpenCommand(IVcsService vcsService, ILogger logger)
        {
            _vcsService = vcsService;
            _logger = logger;
        }

        public async Task<int> Execute(OpenSubOptions options)
        {
            _logger.Information("Opening milestone {Milestone}", options.Milestone);
            await _vcsService.OpenMilestoneAsync(options.RepositoryOwner, options.RepositoryName, options.Milestone).ConfigureAwait(false);

            return 0;
        }
    }
}