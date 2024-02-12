using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitReleaseManager.Core.Commands;
using GitReleaseManager.Core.Model;
using GitReleaseManager.Core.Options;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using Shouldly;

namespace GitReleaseManager.Core.Tests.Commands
{
    [TestFixture]
    public class CreateCommandTests
    {
        private readonly Release _release = new Release { Body = "Release Body", HtmlUrl = "Html Url" };

        private IVcsService _vcsService;
        private ILogger _logger;
        private CreateCommand _command;

        [SetUp]
        public void Setup()
        {
            _vcsService = Substitute.For<IVcsService>();
            _logger = Substitute.For<ILogger>();
            _command = new CreateCommand(_vcsService, _logger);
        }

        public async Task Should_Create_Empty_Release()
        {
            var options = new CreateSubOptions
            {
                RepositoryOwner = "owner",
                RepositoryName = "repository",
                TargetCommitish = "target commitish",
                Prerelease = false,
                AllowEmpty = true,
            };

            var releaseName = options.Name ?? options.Milestone;

            _vcsService.CreateEmptyReleaseAsync(options.RepositoryOwner, options.RepositoryName, options.Name, options.TargetCommitish, options.Prerelease)
                .Returns(_release);

            var result = await _command.ExecuteAsync(options).ConfigureAwait(false);
            result.ShouldBe(0);

            await _vcsService.Received(1).CreateEmptyReleaseAsync(options.RepositoryOwner, options.RepositoryName, releaseName, options.TargetCommitish, options.Prerelease).ConfigureAwait(false);
            _logger.Received(1).Information(Arg.Any<string>());
            _logger.Received(1).Information(Arg.Any<string>(), _release.HtmlUrl);
            _logger.Received(1).Verbose(Arg.Any<string>(), _release.Body);
        }

        [TestCase(null, 2)]
        [TestCase("release", 1)]
        public async Task Should_Create_Release_From_Milestone(string name, int logVerboseCount)
        {
            var options = new CreateSubOptions
            {
                RepositoryOwner = "owner",
                RepositoryName = "repository",
                Milestone = "milestone",
                Name = name,
                TargetCommitish = "target commitish",
                AssetPaths = new List<string>(),
                Prerelease = false,
            };

            var releaseName = options.Name ?? options.Milestone;

            _vcsService.CreateReleaseFromMilestoneAsync(options.RepositoryOwner, options.RepositoryName, options.Milestone, releaseName, options.TargetCommitish, options.AssetPaths, options.Prerelease, options.Template)
                .Returns(_release);

            var result = await _command.ExecuteAsync(options).ConfigureAwait(false);
            result.ShouldBe(0);

            await _vcsService.Received(1).CreateReleaseFromMilestoneAsync(options.RepositoryOwner, options.RepositoryName, options.Milestone, releaseName, options.TargetCommitish, options.AssetPaths, options.Prerelease, options.Template).ConfigureAwait(false);
            _logger.Received(1).Information(Arg.Any<string>());
            _logger.Received(logVerboseCount).Verbose(Arg.Any<string>(), options.Milestone);
            _logger.Received(1).Information(Arg.Any<string>(), _release.HtmlUrl);
            _logger.Received(1).Verbose(Arg.Any<string>(), _release.Body);
        }

        [Test]
        public async Task Should_Create_Release_From_InputFile()
        {
            var options = new CreateSubOptions
            {
                RepositoryOwner = "owner",
                RepositoryName = "repository",
                InputFilePath = "file path",
                TargetCommitish = "target commitish",
                AssetPaths = new List<string>(),
                Prerelease = false,
            };

            _vcsService.CreateReleaseFromInputFileAsync(options.RepositoryOwner, options.RepositoryName, options.Name, options.InputFilePath, options.TargetCommitish, options.AssetPaths, options.Prerelease)
                .Returns(_release);

            var result = await _command.ExecuteAsync(options).ConfigureAwait(false);
            result.ShouldBe(0);

            await _vcsService.Received(1).CreateReleaseFromInputFileAsync(options.RepositoryOwner, options.RepositoryName, options.Name, options.InputFilePath, options.TargetCommitish, options.AssetPaths, options.Prerelease).ConfigureAwait(false);
            _logger.Received(1).Information(Arg.Any<string>());
            _logger.Received(1).Information(Arg.Any<string>(), _release.HtmlUrl);
            _logger.Received(1).Verbose(Arg.Any<string>(), _release.Body);
        }

        [Test]
        public async Task Throws_Exception_When_Both_Milestone_And_Input_File_Specified()
        {
            var options = new CreateSubOptions
            {
                RepositoryName = "repository",
                RepositoryOwner = "owner",
                InputFilePath = "file.path",
                TargetCommitish = "target commitish",
                AssetPaths = new List<string>(),
                Prerelease = false,
                Milestone = "0.5.0",
            };

            Func<Task> action = async () => await _command.ExecuteAsync(options).ConfigureAwait(false);

            var ex = await action.ShouldThrowAsync<InvalidOperationException>().ConfigureAwait(false);
            ex.Message.ShouldBe("Both a milestone and an input file path have been specified. Only one of these arguments may be used at the same time when creating a release!");

            _vcsService.ReceivedCalls().ShouldBeEmpty();
            _logger.ReceivedCalls().ShouldHaveSingleItem();
        }
    }
}