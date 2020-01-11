//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilderIntegrationTests.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.IntegrationTests
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using AutoMapper;
    using GitReleaseManager.Core;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using NUnit.Framework;
    using Serilog;

    [TestFixture]
    public class ReleaseNotesBuilderIntegrationTests
    {
        private IMapper _mapper;
        private string _username;
        private string _password;
        private string _token;

        public TestContext TestContext { get; set; }

        [OneTimeSetUp]
        public void Configure()
        {
            _mapper = AutoMapperConfiguration.Configure();
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            _username = Environment.GetEnvironmentVariable("GITTOOLS_GITHUB_USERNAME");
            _password = Environment.GetEnvironmentVariable("GITTOOLS_GITHUB_PASSWORD");
            _token = Environment.GetEnvironmentVariable("GITTOOLS_GITHUB_TOKEN");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Log.CloseAndFlush();
        }

        [Test]
        [Explicit]
        public async Task SingleMilestone()
        {
            if ((string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password)) && string.IsNullOrEmpty(_token))
            {
                Assert.Inconclusive("Unable to locate credentials for accessing GitHub API");
            }
            else
            {
                var fileSystem = new FileSystem();
                var currentDirectory = Environment.CurrentDirectory;
                var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

                var vcsProvider = new GitHubProvider(_mapper, configuration, _username, _password, _token);
                var releaseNotesBuilder = new ReleaseNotesBuilder(vcsProvider, "Chocolatey", "ChocolateyGUI", "0.12.4", configuration);
                var result = await releaseNotesBuilder.BuildReleaseNotes().ConfigureAwait(false);
                Debug.WriteLine(result);
                ClipBoardHelper.SetClipboard(result);
            }
        }

        [Test]
        [Explicit]
        public async Task SingleMilestone3()
        {
            if ((string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password)) && string.IsNullOrEmpty(_token))
            {
                Assert.Inconclusive("Unable to locate credentials for accessing GitHub API");
            }
            else
            {
                var fileSystem = new FileSystem();
                var currentDirectory = Environment.CurrentDirectory;
                var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

                var vcsProvider = new GitHubProvider(_mapper, configuration, _username, _password, _token);
                var releaseNotesBuilder = new ReleaseNotesBuilder(vcsProvider, "Chocolatey", "ChocolateyGUI", "0.13.0", configuration);
                var result = await releaseNotesBuilder.BuildReleaseNotes().ConfigureAwait(false);
                Debug.WriteLine(result);
                ClipBoardHelper.SetClipboard(result);
            }
        }

        [Test]
        [Explicit]
        public void OctokitTests()
        {
            try
            {
                var client = ClientBuilder.Build();
                Assert.That(client, Is.Not.Null);
            }
            finally
            {
                ClientBuilder.Cleanup();
            }
        }
    }
}