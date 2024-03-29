using System;
using System.Threading.Tasks;
using GitReleaseManager.Core.Commands;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Helpers;
using GitReleaseManager.Core.Options;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using Shouldly;

namespace GitReleaseManager.Core.Tests.Commands
{
    [TestFixture]
    public class ShowConfigCommandTests
    {
        private IFileSystem _fileSystem;
        private ILogger _logger;
        private ShowConfigCommand _command;

        [SetUp]
        public void Setup()
        {
            _fileSystem = Substitute.For<IFileSystem>();
            _logger = Substitute.For<ILogger>();

            var currentDirectory = Environment.CurrentDirectory;
            var configuration = ConfigurationProvider.Provide(currentDirectory, _fileSystem);
            _command = new ShowConfigCommand(_logger, configuration);
        }

        [Test]
        public async Task Should_Execute_Command()
        {
            var options = new ShowConfigSubOptions();

            var result = await _command.ExecuteAsync(options).ConfigureAwait(false);
            result.ShouldBe(0);

            _logger.Received(1).Information(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}