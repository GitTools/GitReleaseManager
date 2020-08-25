// -----------------------------------------------------------------------
// <copyright file="CloseCommandTests.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

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
    public class CloseCommandTests
    {
        private IVcsService _vcsService;
        private ILogger _logger;
        private CloseCommand _command;

        [SetUp]
        public void Setup()
        {
            _vcsService = Substitute.For<IVcsService>();
            _logger = Substitute.For<ILogger>();
            _command = new CloseCommand(_vcsService, _logger);
        }

        [Test]
        public async Task Should_Execute_Command()
        {
            var options = new CloseSubOptions
            {
                RepositoryOwner = "owner",
                RepositoryName = "repository",
                Milestone = "0.1.0",
            };

            _vcsService.CloseMilestoneAsync(options.RepositoryOwner, options.RepositoryName, options.Milestone)
                .Returns(Task.CompletedTask);

            var result = await _command.Execute(options).ConfigureAwait(false);
            result.ShouldBe(0);

            await _vcsService.Received(1).CloseMilestoneAsync(options.RepositoryOwner, options.RepositoryName, options.Milestone).ConfigureAwait(false);
            _logger.Received(1).Information(Arg.Any<string>(), options.Milestone);
        }
    }
}