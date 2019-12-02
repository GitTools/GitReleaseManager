//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilderIntegrationTests.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

namespace GitReleaseManager.Tests
{
    using System;
    using System.Diagnostics;
    using AutoMapper;
    using GitReleaseManager.Core;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using NUnit.Framework;
    using Octokit;

    [TestFixture]
    public class ReleaseNotesBuilderIntegrationTests
    {
        private IMapper _mapper;

        [OneTimeSetUp]
        public void Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Core.Model.Issue, Issue>();
                cfg.CreateMap<Core.Model.Release, Release>();
                cfg.CreateMap<Core.Model.Label, Label>();
                cfg.CreateMap<Core.Model.Milestone, Milestone>();

                cfg.CreateMap<Issue, Core.Model.Issue>();
                cfg.CreateMap<Release, Core.Model.Release>();
                cfg.CreateMap<Label, Core.Model.Label>();
                cfg.CreateMap<Milestone, Core.Model.Milestone>()
                    .AfterMap((src, dest) => dest.Version = src.Version());
            });

            _mapper = config.CreateMapper();
        }

        [Test]
        [Explicit]
        public async Task SingleMilestone()
        {
            var gitHubClient = ClientBuilder.Build();
            var fileSystem = new FileSystem();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(gitHubClient, "Chocolatey", "ChocolateyGUI", _mapper), "Chocolatey", "ChocolateyGUI", "0.12.4", configuration);
            var result = await releaseNotesBuilder.BuildReleaseNotes();
            Debug.WriteLine(result);
            ClipBoardHelper.SetClipboard(result);
        }

        [Test]
        [Explicit]
        public async Task SingleMilestone3()
        {
            var gitHubClient = ClientBuilder.Build();
            var fileSystem = new FileSystem();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(gitHubClient, "Chocolatey", "ChocolateyGUI", _mapper), "Chocolatey", "ChocolateyGUI", "0.13.0", configuration);
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