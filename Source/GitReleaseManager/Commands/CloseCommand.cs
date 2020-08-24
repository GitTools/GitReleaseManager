// -----------------------------------------------------------------------
// <copyright file="CloseCommand.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;
using GitReleaseManager.Core.Options;
using Serilog;

namespace GitReleaseManager.Core.Commands
{
    public class CloseCommand : ICommand<CloseSubOptions>
    {
        private readonly IVcsService _vcsService;
        private readonly ILogger _logger;

        public CloseCommand(IVcsService vcsService, ILogger logger)
        {
            _vcsService = vcsService;
            _logger = logger;
        }

        public async Task<int> Execute(CloseSubOptions options)
        {
            _logger.Information("Closing milestone {Milestone}", options.Milestone);
            await _vcsService.CloseMilestoneAsync(options.RepositoryOwner, options.RepositoryName, options.Milestone).ConfigureAwait(false);

            return 0;
        }
    }
}