// -----------------------------------------------------------------------
// <copyright file="DiscardCommand.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Commands
{
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Options;
    using Serilog;

    public class DiscardCommand : ICommand<DiscardSubOptions>
    {
        private readonly IVcsService _vcsService;
        private readonly ILogger _logger;

        public DiscardCommand(IVcsService vcsService, ILogger logger)
        {
            _vcsService = vcsService;
            _logger = logger;
        }

        public async Task<int> Execute(DiscardSubOptions options)
        {
            _logger.Information("Discarding release {Milestone}", options.Milestone);
            await _vcsService.DiscardReleaseAsync(options.RepositoryOwner, options.RepositoryName, options.Milestone).ConfigureAwait(false);

            return 0;
        }
    }
}