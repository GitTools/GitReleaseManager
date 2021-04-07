using System.Threading.Tasks;
using GitReleaseManager.Core.Commands;
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
            _command = new ShowConfigCommand(_fileSystem, _logger);
        }

        [Test]
        public async Task Should_Execute_Command()
        {
            var options = new ShowConfigSubOptions();

            var result = await _command.Execute(options).ConfigureAwait(false);
            result.ShouldBe(0);

            _logger.Received(1).Information(Arg.Any<string>(), Arg.Any<string>());
        }
    }
}