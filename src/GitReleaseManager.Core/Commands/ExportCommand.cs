using System.IO;
using System.Threading.Tasks;
using GitReleaseManager.Core.Options;
using Serilog;

namespace GitReleaseManager.Core.Commands
{
    public class ExportCommand : ICommand<ExportSubOptions>
    {
        private readonly IVcsService _vcsService;
        private readonly ILogger _logger;

        public ExportCommand(IVcsService vcsService, ILogger logger)
        {
            _vcsService = vcsService;
            _logger = logger;
        }

        public async Task<int> ExecuteAsync(ExportSubOptions options)
        {
            _logger.Information("Exporting release {TagName}", options.TagName);
            var releasesContent = await _vcsService.ExportReleasesAsync(options.RepositoryOwner, options.RepositoryName, options.TagName, options.SkipPrereleases).ConfigureAwait(false);

            using (var sw = new StreamWriter(File.Open(options.FileOutputPath, FileMode.OpenOrCreate)))
            {
                sw.Write(releasesContent);
            }

            return 0;
        }
    }
}