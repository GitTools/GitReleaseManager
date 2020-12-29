// -----------------------------------------------------------------------
// <copyright file="OpenCommand.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Commands
{
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Options;
    using Serilog;

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