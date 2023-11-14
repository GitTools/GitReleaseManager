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
            var vcsOptions = options as BaseVcsOptions;

            if (vcsOptions?.Provider == Model.VcsProvider.GitLab)
            {
                _logger.Error("The 'addasset' command is currently not supported when targeting GitLab.");
                return 1;
            }

            _logger.Information("Uploading assets");
            await _vcsService.AddAssetsAsync(options.RepositoryOwner, options.RepositoryName, options.TagName, options.AssetPaths).ConfigureAwait(false);

            return 0;
        }
    }
}