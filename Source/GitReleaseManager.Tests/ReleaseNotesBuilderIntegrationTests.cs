//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilderIntegrationTests.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using AutoMapper;
    using GitReleaseManager.Core;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class ReleaseNotesBuilderIntegrationTests
    {
        private IMapper _mapper;

        public TestContext TestContext { get; set; }

        [OneTimeSetUp]
        public void Configure()
        {
            _mapper = AutoMapperConfiguration.Configure();
            Logger.WriteError = s => TestContext.WriteLine($"Error: {s}");
            Logger.WriteInfo = s => TestContext.WriteLine($"Info: {s}");
            Logger.WriteWarning = s => TestContext.WriteLine($"Warning: {s}");
        }

        [Test]
        [Explicit]
        public async Task SingleMilestone()
        {
            var fileSystem = new FileSystem();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

            var vcsProvider = new GitHubProvider(_mapper, configuration, "username", "password", "token");
            var releaseNotesBuilder = new ReleaseNotesBuilder(vcsProvider, "Chocolatey", "ChocolateyGUI", "0.12.4", configuration);
            var result = await releaseNotesBuilder.BuildReleaseNotes().ConfigureAwait(false);
            Debug.WriteLine(result);
            ClipBoardHelper.SetClipboard(result);
        }

        [Test]
        [Explicit]
        public async Task SingleMilestone3()
        {
            var fileSystem = new FileSystem();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

            var vcsProvider = new GitHubProvider(_mapper, configuration, "username", "password", "token");
            var releaseNotesBuilder = new ReleaseNotesBuilder(vcsProvider, "Chocolatey", "ChocolateyGUI", "0.13.0", configuration);
            var result = await releaseNotesBuilder.BuildReleaseNotes().ConfigureAwait(false);
            Debug.WriteLine(result);
            ClipBoardHelper.SetClipboard(result);
        }

        [Test]
        [Explicit]
        public void OctokitTests()
        {
            try
            {
                ClientBuilder.Build();
            }
            finally
            {
                ClientBuilder.Cleanup();
            }
        }
    }
}