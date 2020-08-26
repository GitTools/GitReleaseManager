// -----------------------------------------------------------------------
// <copyright file="VcsServiceTests.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Model;
using GitReleaseManager.Core.Provider;
using GitReleaseManager.Core.ReleaseNotes;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using Shouldly;
using ItemState = GitReleaseManager.Core.Model.ItemState;
using ItemStateFilter = GitReleaseManager.Core.Model.ItemStateFilter;
using Label = GitReleaseManager.Core.Model.Label;
using Milestone = GitReleaseManager.Core.Model.Milestone;
using NotFoundException = GitReleaseManager.Core.Exceptions.NotFoundException;
using Release = GitReleaseManager.Core.Model.Release;

namespace GitReleaseManager.Core.Tests
{
    public class VcsServiceTests
    {
        private const string _owner = "owner";
        private const string _repository = "repository";
        private const int _milestoneNumber = 1;
        private const string _milestoneTitle = "0.1.0";
        private const string _tagName = "0.1.0";
        private const string _releaseNotes = "Release Notes";

        private const string _unableToFoundMilestoneMessage = "Unable to find a {State} milestone with title '{Title}' on '{Owner}/{Repository}'";
        private const string _unableToFoundReleaseMessage = "Unable to find a release with tag '{TagName}' on '{Owner}/{Repository}'";

        private readonly string _tempPath = Path.GetTempPath();
        private readonly List<string> _assets = new List<string>();
        private readonly List<string> _files = new List<string>();
        private readonly NotFoundException _notFoundException = new NotFoundException("NotFound");

        private IReleaseNotesExporter _releaseNotesExporter;
        private IReleaseNotesBuilder _releaseNotesBuilder;
        private ILogger _logger;
        private IVcsProvider _vcsProvider;
        private Config _configuration;
        private VcsService _vcsService;

        private string _releaseNotesFilePath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _releaseNotesFilePath = Path.Combine(_tempPath, "ReleaseNotes.txt");

            for (int i = 0; i < 3; i++)
            {
                _assets.Add(Path.Combine(_tempPath, $"Asset{i + 1}.txt"));
            }

            _files.Add(_releaseNotesFilePath);
            _files.AddRange(_assets);

            foreach (var file in _files)
            {
                if (!File.Exists(file))
                {
                    File.WriteAllText(file, _releaseNotes);
                }
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            foreach (var file in _files)
            {
                if (!File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        [SetUp]
        public void Setup()
        {
            _releaseNotesExporter = Substitute.For<IReleaseNotesExporter>();
            _releaseNotesBuilder = Substitute.For<IReleaseNotesBuilder>();
            _logger = Substitute.For<ILogger>();
            _vcsProvider = Substitute.For<IVcsProvider>();
            _configuration = new Config();
            _vcsService = new VcsService(_vcsProvider, _logger, _releaseNotesBuilder, _releaseNotesExporter, _configuration);
        }

        [Test]
        public async Task Should_Add_Assets()
        {
            var release = new Release { Assets = new List<ReleaseAsset>() };

            var assetsCount = _assets.Count;

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(release);

            await _vcsService.AddAssetsAsync(_owner, _repository, _tagName, _assets).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().DeleteAssetAsync(_owner, _repository, Arg.Any<int>()).ConfigureAwait(false);
            await _vcsProvider.Received(assetsCount).UploadAssetAsync(release, Arg.Any<ReleaseAssetUpload>()).ConfigureAwait(false);

            _logger.DidNotReceive().Warning(Arg.Any<string>(), Arg.Any<string>());
            _logger.Received(assetsCount).Verbose(Arg.Any<string>(), Arg.Any<string>(), _tagName, _owner, _repository);
            _logger.Received(assetsCount).Debug(Arg.Any<string>(), Arg.Any<ReleaseAssetUpload>());
        }

        [Test]
        public async Task Should_Add_Assets_With_Deleting_Existing_Assets()
        {
            var releaseAsset = new ReleaseAsset { Id = 1, Name = "Asset1.txt" };
            var release = new Release { Assets = new List<ReleaseAsset> { releaseAsset } };

            var releaseAssetsCount = release.Assets.Count;
            var assetsCount = _assets.Count;

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(release);

            await _vcsService.AddAssetsAsync(_owner, _repository, _tagName, _assets).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.Received(releaseAssetsCount).DeleteAssetAsync(_owner, _repository, releaseAsset.Id).ConfigureAwait(false);
            await _vcsProvider.Received(assetsCount).UploadAssetAsync(release, Arg.Any<ReleaseAssetUpload>()).ConfigureAwait(false);

            _logger.Received(releaseAssetsCount).Warning(Arg.Any<string>(), Arg.Any<string>());
            _logger.Received(assetsCount).Verbose(Arg.Any<string>(), Arg.Any<string>(), _tagName, _owner, _repository);
            _logger.Received(assetsCount).Debug(Arg.Any<string>(), Arg.Any<ReleaseAssetUpload>());
        }

        [Test]
        public async Task Should_Throw_Exception_On_Adding_Assets_When_Asset_File_Not_Exists()
        {
            var release = new Release();
            var assetFilePath = Path.Combine(_tempPath, "AssetNotExists.txt");

            _assets[0] = assetFilePath;

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(release);

            var ex = await Should.ThrowAsync<FileNotFoundException>(() => _vcsService.AddAssetsAsync(_owner, _repository, _tagName, _assets)).ConfigureAwait(false);
            ex.Message.ShouldContain(assetFilePath);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().DeleteAssetAsync(_owner, _repository, Arg.Any<int>()).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().UploadAssetAsync(release, Arg.Any<ReleaseAssetUpload>()).ConfigureAwait(false);
        }

        [TestCaseSource(nameof(Assets_TestCases))]
        public async Task Should_Do_Nothing_On_Missing_Assets(IList<string> assets)
        {
            await _vcsService.AddAssetsAsync(_owner, _repository, _tagName, assets).ConfigureAwait(false);

            await _vcsProvider.DidNotReceive().GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().DeleteAssetAsync(_owner, _repository, Arg.Any<int>()).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().UploadAssetAsync(Arg.Any<Release>(), Arg.Any<ReleaseAssetUpload>()).ConfigureAwait(false);
        }

        public static IEnumerable Assets_TestCases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new List<string>());
        }

        [Test]
        public async Task Should_Create_Labels()
        {
            var labels = new List<Label>
            {
                new Label { Name = "Bug" },
                new Label { Name = "Feature" },
                new Label { Name = "Improvement" },
            };

            _vcsProvider.GetLabelsAsync(_owner, _repository)
                .Returns(Task.FromResult((IEnumerable<Label>)labels));

            _vcsProvider.DeleteLabelAsync(_owner, _repository, Arg.Any<string>())
                .Returns(Task.CompletedTask);

            _vcsProvider.CreateLabelAsync(_owner, _repository, Arg.Any<Label>())
                .Returns(Task.CompletedTask);

            await _vcsService.CreateLabelsAsync(_owner, _repository).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetLabelsAsync(_owner, _repository).ConfigureAwait(false);
            await _vcsProvider.Received(labels.Count).DeleteLabelAsync(_owner, _repository, Arg.Any<string>()).ConfigureAwait(false);
            await _vcsProvider.Received(_configuration.Labels.Count).CreateLabelAsync(_owner, _repository, Arg.Any<Label>()).ConfigureAwait(false);

            _logger.Received(1).Verbose(Arg.Any<string>(), _owner, _repository);
            _logger.Received(2).Verbose(Arg.Any<string>());
            _logger.Received(2).Debug(Arg.Any<string>(), Arg.Any<IEnumerable<Label>>());
        }

        [Test]
        public async Task Should_Log_An_Warning_When_Labels_Not_Configured()
        {
            _configuration.Labels.Clear();

            await _vcsService.CreateLabelsAsync(_owner, _repository).ConfigureAwait(false);

            _logger.Received(1).Warning(Arg.Any<string>());
        }

        [Test]
        public async Task Should_Close_Milestone()
        {
            var milestone = new Milestone { Number = _milestoneNumber };

            _vcsProvider.GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Open)
                .Returns(Task.FromResult(milestone));

            _vcsProvider.SetMilestoneStateAsync(_owner, _repository, milestone.Number, ItemState.Closed)
                .Returns(Task.CompletedTask);

            await _vcsService.CloseMilestoneAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Open).ConfigureAwait(false);
            await _vcsProvider.Received(1).SetMilestoneStateAsync(_owner, _repository, milestone.Number, ItemState.Closed).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Log_An_Warning_On_Closing_When_Milestone_Cannot_Be_Found()
        {
            _vcsProvider.GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Open)
                .Returns(Task.FromException<Milestone>(_notFoundException));

            await _vcsService.CloseMilestoneAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Open).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().SetMilestoneStateAsync(_owner, _repository, _milestoneNumber, ItemState.Closed).ConfigureAwait(false);
            _logger.Received(1).Warning(_unableToFoundMilestoneMessage, "open", _milestoneTitle, _owner, _repository);
        }

        [Test]
        public async Task Should_Open_Milestone()
        {
            var milestone = new Milestone { Number = _milestoneNumber };

            _vcsProvider.GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Closed)
                .Returns(Task.FromResult(milestone));

            _vcsProvider.SetMilestoneStateAsync(_owner, _repository, milestone.Number, ItemState.Open)
                .Returns(Task.CompletedTask);

            await _vcsService.OpenMilestoneAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Closed).ConfigureAwait(false);
            await _vcsProvider.Received(1).SetMilestoneStateAsync(_owner, _repository, milestone.Number, ItemState.Open).ConfigureAwait(false);
            _logger.Received(2).Verbose(Arg.Any<string>(), _milestoneTitle, _owner, _repository);
        }

        [Test]
        public async Task Should_Log_An_Warning_On_Opening_When_Milestone_Cannot_Be_Found()
        {
            _vcsProvider.GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Closed)
                .Returns(Task.FromException<Milestone>(_notFoundException));

            await _vcsService.OpenMilestoneAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Closed).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().SetMilestoneStateAsync(_owner, _repository, _milestoneNumber, ItemState.Open).ConfigureAwait(false);
            _logger.Received(1).Warning(_unableToFoundMilestoneMessage, "closed", _milestoneTitle, _owner, _repository);
        }

        [Test]
        public async Task Should_Create_Release_From_Milestone()
        {
            var release = new Release();

            _releaseNotesBuilder.BuildReleaseNotes(_owner, _repository, _milestoneTitle, ReleaseNotesTemplate.Default)
                .Returns(Task.FromResult(_releaseNotes));

            _vcsProvider.GetReleaseAsync(_owner, _repository, _milestoneTitle)
                .Returns(Task.FromException<Release>(_notFoundException));

            _vcsProvider.CreateReleaseAsync(_owner, _repository, Arg.Any<Release>())
                .Returns(Task.FromResult(release));

            var result = await _vcsService.CreateReleaseFromMilestoneAsync(_owner, _repository, _milestoneTitle, _milestoneTitle, null, null, false).ConfigureAwait(false);
            result.ShouldBeSameAs(release);

            await _releaseNotesBuilder.Received(1).BuildReleaseNotes(_owner, _repository, _milestoneTitle, ReleaseNotesTemplate.Default).ConfigureAwait(false);
            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);
            await _vcsProvider.Received(1).CreateReleaseAsync(_owner, _repository, Arg.Is<Release>(o =>
                o.Body == _releaseNotes &&
                o.Name == _milestoneTitle &&
                o.TagName == _milestoneTitle)).ConfigureAwait(false);

            _logger.Received(1).Verbose(Arg.Any<string>(), _milestoneTitle, _owner, _repository);
            _logger.Received(1).Debug(Arg.Any<string>(), Arg.Any<Release>());
        }

        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public async Task Should_Update_Published_Release_On_Creating_Release_From_Milestone(bool isDraft, bool updatePublishedRelease)
        {
            var release = new Release { Draft = isDraft };

            _configuration.Create.AllowUpdateToPublishedRelease = updatePublishedRelease;

            _releaseNotesBuilder.BuildReleaseNotes(_owner, _repository, _milestoneTitle, ReleaseNotesTemplate.Default)
                .Returns(Task.FromResult(_releaseNotes));

            _vcsProvider.GetReleaseAsync(_owner, _repository, _milestoneTitle)
                .Returns(Task.FromResult(release));

            _vcsProvider.UpdateReleaseAsync(_owner, _repository, release)
                .Returns(Task.FromResult(new Release()));

            var result = await _vcsService.CreateReleaseFromMilestoneAsync(_owner, _repository, _milestoneTitle, _milestoneTitle, null, null, false).ConfigureAwait(false);
            result.ShouldBeSameAs(release);

            await _releaseNotesBuilder.Received(1).BuildReleaseNotes(_owner, _repository, _milestoneTitle, ReleaseNotesTemplate.Default).ConfigureAwait(false);
            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);
            await _vcsProvider.Received(1).UpdateReleaseAsync(_owner, _repository, release).ConfigureAwait(false);

            _logger.Received(1).Warning(Arg.Any<string>(), _milestoneTitle);
            _logger.Received(1).Verbose(Arg.Any<string>(), _milestoneTitle, _owner, _repository);
            _logger.Received(1).Debug(Arg.Any<string>(), release);
        }

        [Test]
        public async Task Should_Throw_Exception_While_Updating_Published_Release_On_Creating_Release_From_Milestone()
        {
            var release = new Release { Draft = false };

            _configuration.Create.AllowUpdateToPublishedRelease = false;

            _releaseNotesBuilder.BuildReleaseNotes(_owner, _repository, _milestoneTitle, ReleaseNotesTemplate.Default)
                .Returns(Task.FromResult(_releaseNotes));

            _vcsProvider.GetReleaseAsync(_owner, _repository, _milestoneTitle)
                .Returns(Task.FromResult(release));

            var ex = await Should.ThrowAsync<InvalidOperationException>(() => _vcsService.CreateReleaseFromMilestoneAsync(_owner, _repository, _milestoneTitle, _milestoneTitle, null, null, false)).ConfigureAwait(false);
            ex.Message.ShouldBe($"Release with tag '{_milestoneTitle}' not in draft state, so not updating");

            await _releaseNotesBuilder.Received(1).BuildReleaseNotes(_owner, _repository, _milestoneTitle, ReleaseNotesTemplate.Default).ConfigureAwait(false);
            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Create_Release_From_InputFile()
        {
            var release = new Release();

            _vcsProvider.GetReleaseAsync(_owner, _repository, _milestoneTitle)
                .Returns(Task.FromException<Release>(_notFoundException));

            _vcsProvider.CreateReleaseAsync(_owner, _repository, Arg.Any<Release>())
                .Returns(Task.FromResult(release));

            var result = await _vcsService.CreateReleaseFromInputFileAsync(_owner, _repository, _milestoneTitle, _releaseNotesFilePath, null, null, false).ConfigureAwait(false);
            result.ShouldBeSameAs(release);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);
            await _vcsProvider.Received(1).CreateReleaseAsync(_owner, _repository, Arg.Is<Release>(o =>
                o.Body == _releaseNotes &&
                o.Name == _milestoneTitle &&
                o.TagName == _milestoneTitle)).ConfigureAwait(false);

            _logger.Received(1).Verbose(Arg.Any<string>(), _milestoneTitle, _owner, _repository);
            _logger.Received(1).Debug(Arg.Any<string>(), Arg.Any<Release>());
        }

        [TestCase(true, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public async Task Should_Update_Published_Release_On_Creating_Release_From_InputFile(bool isDraft, bool updatePublishedRelease)
        {
            var release = new Release { Draft = isDraft };

            _configuration.Create.AllowUpdateToPublishedRelease = updatePublishedRelease;

            _vcsProvider.GetReleaseAsync(_owner, _repository, _milestoneTitle)
                .Returns(Task.FromResult(release));

            _vcsProvider.UpdateReleaseAsync(_owner, _repository, release)
                .Returns(Task.FromResult(new Release()));

            var result = await _vcsService.CreateReleaseFromInputFileAsync(_owner, _repository, _milestoneTitle, _releaseNotesFilePath, null, null, false).ConfigureAwait(false);
            result.ShouldBeSameAs(release);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);
            await _vcsProvider.Received(1).UpdateReleaseAsync(_owner, _repository, release).ConfigureAwait(false);

            _logger.Received(1).Warning(Arg.Any<string>(), _milestoneTitle);
            _logger.Received(1).Verbose(Arg.Any<string>(), _milestoneTitle, _owner, _repository);
            _logger.Received(1).Debug(Arg.Any<string>(), release);
        }

        [Test]
        public async Task Should_Throw_Exception_While_Updating_Published_Release_On_Creating_Release_From_InputFile()
        {
            var release = new Release { Draft = false };

            _configuration.Create.AllowUpdateToPublishedRelease = false;

            _vcsProvider.GetReleaseAsync(_owner, _repository, _milestoneTitle)
                .Returns(Task.FromResult(release));

            var ex = await Should.ThrowAsync<InvalidOperationException>(() => _vcsService.CreateReleaseFromInputFileAsync(_owner, _repository, _milestoneTitle, _releaseNotesFilePath, null, null, false)).ConfigureAwait(false);
            ex.Message.ShouldBe($"Release with tag '{_milestoneTitle}' not in draft state, so not updating");

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _milestoneTitle).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_Exception_Creating_Release_From_InputFile()
        {
            var fileName = "NonExistingReleaseNotes.txt";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            var ex = await Should.ThrowAsync<FileNotFoundException>(() => _vcsService.CreateReleaseFromInputFileAsync(_owner, _repository, _milestoneTitle, filePath, null, null, false)).ConfigureAwait(false);
            ex.Message.ShouldBe("Unable to locate input file.");
            ex.FileName.ShouldBe(fileName);
        }

        [Test]
        public async Task Should_Delete_Draft_Release()
        {
            var release = new Release
            {
                Id = 1,
                Draft = true,
            };

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromResult(release));

            _vcsProvider.DeleteReleaseAsync(_owner, _repository, release.Id)
                .Returns(Task.CompletedTask);

            await _vcsService.DiscardReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.Received(1).DeleteReleaseAsync(_owner, _repository, release.Id).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Not_Delete_Published_Release()
        {
            var release = new Release { Id = 1 };

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromResult(release));

            await _vcsService.DiscardReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().DeleteReleaseAsync(_owner, _repository, release.Id).ConfigureAwait(false);
            _logger.Received(1).Warning(Arg.Any<string>(), _tagName);
        }

        [Test]
        public async Task Should_Log_An_Warning_On_Deleting_Release_For_Non_Existing_Tag()
        {
            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromException<Release>(_notFoundException));

            await _vcsService.DiscardReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);

            await _vcsProvider.Received().GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceiveWithAnyArgs().DeleteReleaseAsync(_owner, _repository, default).ConfigureAwait(false);
            _logger.Received(1).Warning(_unableToFoundReleaseMessage, _tagName, _owner, _repository);
        }

        [Test]
        public async Task Should_Publish_Release()
        {
            var release = new Release { Id = 1 };

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromResult(release));

            _vcsProvider.PublishReleaseAsync(_owner, _repository, _tagName, release.Id)
                .Returns(Task.CompletedTask);

            await _vcsService.PublishReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.Received(1).PublishReleaseAsync(_owner, _repository, _tagName, release.Id).ConfigureAwait(false);
            _logger.Received(1).Verbose(Arg.Any<string>(), _tagName, _owner, _repository);
            _logger.Received(1).Debug(Arg.Any<string>(), Arg.Any<Release>());
        }

        [Test]
        public async Task Should_Log_An_Warning_On_Publishing_Release_For_Non_Existing_Tag()
        {
            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromException<Release>(_notFoundException));

            await _vcsService.PublishReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);

            await _vcsProvider.Received().GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceiveWithAnyArgs().PublishReleaseAsync(_owner, _repository, _tagName, default).ConfigureAwait(false);
            _logger.Received(1).Warning(Arg.Any<string>(), _tagName, _owner, _repository);
        }

        [Test]
        public async Task Should_Get_Release_Notes()
        {
            var releases = Enumerable.Empty<Release>();
            var releaseNotes = "Release Notes";

            _vcsProvider.GetReleasesAsync(_owner, _repository)
                .Returns(Task.FromResult(releases));

            _releaseNotesExporter.ExportReleaseNotes(Arg.Any<IEnumerable<Release>>())
                .Returns(releaseNotes);

            var result = await _vcsService.ExportReleasesAsync(_owner, _repository, null).ConfigureAwait(false);
            result.ShouldBeSameAs(releaseNotes);

            await _vcsProvider.DidNotReceive().GetReleaseAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ConfigureAwait(false);
            await _vcsProvider.Received(1).GetReleasesAsync(_owner, _repository).ConfigureAwait(false);
            _logger.Received(1).Verbose(Arg.Any<string>(), _owner, _repository);
            _releaseNotesExporter.Received(1).ExportReleaseNotes(Arg.Any<IEnumerable<Release>>());
        }

        [Test]
        public async Task Should_Get_Release_Notes_For_Tag()
        {
            var release = new Release();

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromResult(release));

            _releaseNotesExporter.ExportReleaseNotes(Arg.Any<IEnumerable<Release>>())
                .Returns(_releaseNotes);

            var result = await _vcsService.ExportReleasesAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            result.ShouldBeSameAs(_releaseNotes);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().GetReleasesAsync(Arg.Any<string>(), Arg.Any<string>()).ConfigureAwait(false);
            _logger.Received(1).Verbose(Arg.Any<string>(), _owner, _repository, _tagName);
            _releaseNotesExporter.Received(1).ExportReleaseNotes(Arg.Any<IEnumerable<Release>>());
        }

        [Test]
        public async Task Should_Get_Default_Release_Notes_For_Non_Existent_Tag()
        {
            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromException<Release>(_notFoundException));

            _releaseNotesExporter.ExportReleaseNotes(Arg.Any<IEnumerable<Release>>())
                .Returns(_releaseNotes);

            var result = await _vcsService.ExportReleasesAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            result.ShouldBeSameAs(_releaseNotes);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            _logger.Received(1).Warning(_unableToFoundReleaseMessage, _tagName, _owner, _repository);
            _releaseNotesExporter.Received(1).ExportReleaseNotes(Arg.Any<IEnumerable<Release>>());
        }
    }
}