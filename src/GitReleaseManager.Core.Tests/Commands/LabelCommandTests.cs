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
    public class LabelCommandTests
    {
        private IVcsService _vcsService;
        private ILogger _logger;
        private LabelCommand _command;

        [SetUp]
        public void Setup()
        {
            _vcsService = Substitute.For<IVcsService>();
            _logger = Substitute.For<ILogger>();
            _command = new LabelCommand(_vcsService, _logger);
        }

        [Test]
        public async Task Should_Execute_Command()
        {
            var options = new LabelSubOptions
            {
                RepositoryOwner = "owner",
                RepositoryName = "repository",
            };

            _vcsService.CreateLabelsAsync(options.RepositoryOwner, options.RepositoryName)
                .Returns(Task.CompletedTask);

            var result = await _command.ExecuteAsync(options).ConfigureAwait(false);
            result.ShouldBe(0);

            await _vcsService.Received(1).CreateLabelsAsync(options.RepositoryOwner, options.RepositoryName).ConfigureAwait(false);
            _logger.Received(1).Information(Arg.Any<string>());
        }
    }
}