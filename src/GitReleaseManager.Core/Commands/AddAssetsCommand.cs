using System.Threading.Tasks;
using GitReleaseManager.Core.Options;
using Serilog;

namespace GitReleaseManager.Core.Commands
{
    public class AddAssetsCommand : ICommand<AddAssetSubOptions>
    {
        private readonly IVcsService _vcsService;
        private readonly ILogger _logger;

        public AddAssetsCommand(IVcsService vcsService, ILogger logger)
        {
            _vcsService = vcsService;
            _logger = logger;
        }

        public async Task<int> ExecuteAsync(AddAssetSubOptions options)
        {
            _logger.Information("Uploading assets");
            await _vcsService.AddAssetsAsync(options.RepositoryOwner, options.RepositoryName, options.TagName, options.AssetPaths).ConfigureAwait(false);

            return 0;
        }
    }
}