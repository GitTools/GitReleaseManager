using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
            _targetDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (!Directory.Exists(_targetDirectory))
            {
                Directory.CreateDirectory(_targetDirectory);
            }
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

        [Test]
        public async Task Should_Not_Execute_Command_For_Legacy_FileName()
        {
            var options = new InitSubOptions { TargetDirectory = _targetDirectory };

            var configFilePath = Path.Combine(_targetDirectory, "GitReleaseManager.yaml");
            var configNewFilePath = Path.Combine(_targetDirectory, "GitReleaseManager.yml");
            if (File.Exists(configNewFilePath))
            {
                File.Delete(configNewFilePath);
            }

            File.WriteAllText(configFilePath, "s");
            var expectedHash = GetHash(configFilePath);

            var result = await _command.Execute(options).ConfigureAwait(false);
            result.ShouldBe(0); // Should this perhaps return 1

            var actualHash = GetHash(configFilePath);
            actualHash.ShouldBe(expectedHash);
            File.Exists(configNewFilePath).ShouldBeFalse();
        }

        private static string GetHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(File.ReadAllBytes(filePath));

                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
                }

                return builder.ToString();
            }
        }
    }
}