// -----------------------------------------------------------------------
// <copyright file="InitCommand.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Commands
{
    using System;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using GitReleaseManager.Core.Options;
    using Serilog;

    public class InitCommand : ICommand<InitSubOptions>
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public InitCommand(IFileSystem fileSystem, ILogger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public Task<int> Execute(InitSubOptions options)
        {
            _logger.Information("Creating sample configuration file");
            var directory = options.TargetDirectory ?? Environment.CurrentDirectory;
            ConfigurationProvider.WriteSample(directory, _fileSystem);

            return Task.FromResult(0);
        }
    }
}