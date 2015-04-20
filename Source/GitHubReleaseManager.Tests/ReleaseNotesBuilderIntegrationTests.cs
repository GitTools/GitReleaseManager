//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilderIntegrationTests.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Tests
{
    using System;
    using System.Diagnostics;
    using GitHubReleaseManager.Core;
    using GitHubReleaseManager.Core.Configuration;
    using GitHubReleaseManager.Core.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class ReleaseNotesBuilderIntegrationTests
    {
        [Test]
        [Explicit]
        public async void SingleMilestone()
        {
            var gitHubClient = ClientBuilder.Build();
            var fileSystem = new FileSystem();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(gitHubClient, "Chocolatey", "ChocolateyGUI"), "Chocolatey", "ChocolateyGUI", "0.12.4", configuration);
            var result = await releaseNotesBuilder.BuildReleaseNotes();
            Debug.WriteLine(result);
            ClipBoardHelper.SetClipboard(result);
        }

        [Test]
        [Explicit]
        public async void SingleMilestone3()
        {
            var gitHubClient = ClientBuilder.Build();
            var fileSystem = new FileSystem();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(gitHubClient, "Chocolatey", "ChocolateyGUI"), "Chocolatey", "ChocolateyGUI", "0.13.0", configuration);
            var result = await releaseNotesBuilder.BuildReleaseNotes();
            Debug.WriteLine(result);
            ClipBoardHelper.SetClipboard(result);
        }

        [Test]
        [Explicit]
        public void OctokitTests()
        {
            ClientBuilder.Build();
        }
    }
}