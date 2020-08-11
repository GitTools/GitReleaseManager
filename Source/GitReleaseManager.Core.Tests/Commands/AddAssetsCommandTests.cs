using System.Collections.Generic;
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
    public class AddAssetsCommandTests
    {
        private IVcsService _vcsService;
        private ILogger _logger;
        private AddAssetsCommand _command;

        [SetUp]
        public void Setup()
        {
            _vcsService = Substitute.For<IVcsService>();
            _logger = Substitute.For<ILogger>();
            _command = new AddAssetsCommand(_vcsService, _logger);
        }

        [Test]
        public async Task Should_Execute_Command()
        {
            var options = new AddAssetSubOptions
            {
                RepositoryOwner = "owner",
                RepositoryName = "repository",
                TagName = "0.1.0",
                AssetPaths = new List<string>(),
            };

            _vcsService.AddAssetsAsync(options.RepositoryOwner, options.RepositoryName, options.TagName, options.AssetPaths).
                Returns(Task.CompletedTask);

            var result = await _command.Execute(options).ConfigureAwait(false);
            result.ShouldBe(0);

            await _vcsService.Received(1).AddAssetsAsync(options.RepositoryOwner, options.RepositoryName, options.TagName, options.AssetPaths).ConfigureAwait(false);
            _logger.Received(1).Information(Arg.Any<string>());
        }
    }
}