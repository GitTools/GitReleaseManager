// -----------------------------------------------------------------------
// <copyright file="ExportCommandTests.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

using System.IO;
using System.Threading.Tasks;
using GitReleaseManager.Core.Commands;
using GitReleaseManager.Core.Options;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using Shouldly;

namespace GitReleaseManager.Core.Tests.Commands
{
    [TestFixture]
    public class ExportCommandTests
    {
        private IVcsService _vcsService;
        private ILogger _logger;
        private ExportCommand _command;
        private string _fileOutputPath;

        [SetUp]
        public void Setup()
        {
            _vcsService = Substitute.For<IVcsService>();
            _logger = Substitute.For<ILogger>();
            _command = new ExportCommand(_vcsService, _logger);
            _fileOutputPath = Path.Combine(Path.GetTempPath(), "ReleaseExport.txt");
        }

        [Test]
        public async Task Should_Execute_Command()
        {
            var options = new ExportSubOptions
            {
                RepositoryOwner = "owner",
                RepositoryName = "repository",
                TagName = "0.1.0",
                FileOutputPath = _fileOutputPath,
            };

            var releaseText = "releaseText";

            _vcsService.ExportReleasesAsync(options.RepositoryOwner, options.RepositoryName, options.TagName)
                .Returns(releaseText);

            var result = await _command.Execute(options).ConfigureAwait(false);
            result.ShouldBe(0);

            var exportFileExists = File.Exists(_fileOutputPath);
            exportFileExists.ShouldBeTrue();

            var exportFileContent = File.ReadAllText(_fileOutputPath);
            exportFileContent.ShouldBe(releaseText);

            await _vcsService.Received(1).ExportReleasesAsync(options.RepositoryOwner, options.RepositoryName, options.TagName).ConfigureAwait(false);
            _logger.Received(1).Information(Arg.Any<string>(), options.TagName);

            if (exportFileExists)
            {
                File.Delete(_fileOutputPath);
            }
        }
    }
}