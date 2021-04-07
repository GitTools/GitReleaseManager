using System.IO;
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
    public class InitCommandTests
    {
        private IFileSystem _fileSystem;
        private ILogger _logger;
        private InitCommand _command;
        private string _targetDirectory;

        [SetUp]
        public void Setup()
        {
            _fileSystem = new FileSystem(Substitute.For<BaseSubOptions>());
            _logger = Substitute.For<ILogger>();
            _command = new InitCommand(_fileSystem, _logger);
            _targetDirectory = Path.GetTempPath();
        }

        [Test]
        public async Task Should_Execute_Command()
        {
            var options = new InitSubOptions { TargetDirectory = _targetDirectory };

            var result = await _command.Execute(options).ConfigureAwait(false);
            result.ShouldBe(0);

            var configFilePath = Path.Combine(_targetDirectory, "GitReleaseManager.yml");
            var configFileExists = File.Exists(configFilePath);
            configFileExists.ShouldBeTrue();

            _logger.Received(1).Information(Arg.Any<string>());

            if (configFileExists)
            {
                File.Delete(configFilePath);
            }
        }
    }
}