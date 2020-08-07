using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Provider;
using GitReleaseManager.Core.ReleaseNotes;
using NSubstitute;
using NUnit.Framework;
using Octokit;
using Serilog;
using Shouldly;
using ItemState = GitReleaseManager.Core.Model.ItemState;
using ItemStateFilter = GitReleaseManager.Core.Model.ItemStateFilter;
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

        private readonly NotFoundException _notFoundException = new NotFoundException("NotFound");

        private IReleaseNotesExporter _releaseNotesExporter;
        private IReleaseNotesBuilder _releaseNotesBuilder;
        private IMapper _mapper;
        private ILogger _logger;
        private IGitHubClient _gitHubClient;
        private IVcsProvider _vcsProvider;
        private Config _configuration;
        private VcsService _vcsService;

        [SetUp]
        public void Setup()
        {
            _releaseNotesExporter = Substitute.For<IReleaseNotesExporter>();
            _releaseNotesBuilder = Substitute.For<IReleaseNotesBuilder>();
            _mapper = Substitute.For<IMapper>();
            _logger = Substitute.For<ILogger>();
            _gitHubClient = Substitute.For<IGitHubClient>();
            _vcsProvider = Substitute.For<IVcsProvider>();
            _configuration = new Config();
            _vcsService = new VcsService(_vcsProvider, _gitHubClient, _logger, _mapper, _releaseNotesBuilder, _releaseNotesExporter, _configuration);
        }

        [Test]
        public async Task Should_Close_A_Milestone()
        {
            var milestone = new Milestone { Number = _milestoneNumber };

            _vcsProvider.GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Open)
                .Returns(Task.FromResult(milestone));

            _vcsProvider.SetMilestoneStateAsync(_owner, _repository, milestone.Number, ItemState.Closed)
                .Returns(Task.CompletedTask);

            await _vcsService.CloseMilestone(_owner, _repository, _milestoneTitle).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Open).ConfigureAwait(false);
            await _vcsProvider.Received(1).SetMilestoneStateAsync(_owner, _repository, milestone.Number, ItemState.Closed).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Log_An_Error_On_Closing_When_A_Milestone_Cannot_Be_Found()
        {
            _vcsProvider.GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Open)
                .Returns(Task.FromException<Milestone>(_notFoundException));

            await _vcsService.CloseMilestone(_owner, _repository, _milestoneTitle).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Open).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().SetMilestoneStateAsync(_owner, _repository, _milestoneNumber, ItemState.Closed).ConfigureAwait(false);
            _logger.Received(1).Error(Arg.Any<string>(), _milestoneTitle);
        }

        [Test]
        public async Task Should_Open_A_Milestone()
        {
            var milestone = new Milestone { Number = _milestoneNumber };

            _vcsProvider.GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Closed)
                .Returns(Task.FromResult(milestone));

            _vcsProvider.SetMilestoneStateAsync(_owner, _repository, milestone.Number, ItemState.Open)
                .Returns(Task.CompletedTask);

            await _vcsService.OpenMilestone(_owner, _repository, _milestoneTitle).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Closed).ConfigureAwait(false);
            await _vcsProvider.Received(1).SetMilestoneStateAsync(_owner, _repository, milestone.Number, ItemState.Open).ConfigureAwait(false);
            _logger.Received(2).Verbose(Arg.Any<string>(), _milestoneTitle, _owner, _repository);
        }

        [Test]
        public async Task Should_Log_An_Error_On_Opening_When_A_Milestone_Cannot_Be_Found()
        {
            _vcsProvider.GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Closed)
                .Returns(Task.FromException<Milestone>(_notFoundException));

            await _vcsService.OpenMilestone(_owner, _repository, _milestoneTitle).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetMilestoneAsync(_owner, _repository, _milestoneTitle, ItemStateFilter.Closed).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().SetMilestoneStateAsync(_owner, _repository, _milestoneNumber, ItemState.Open).ConfigureAwait(false);
            _logger.Received(1).Error(Arg.Any<string>(), _milestoneTitle);
        }

        // ----------------------------------------------------------------------------------------------------

        [Test]
        public async Task Should_Delete_A_Draft_Release()
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

            await _vcsService.DiscardRelease(_owner, _repository, _tagName).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.Received(1).DeleteReleaseAsync(_owner, _repository, release.Id).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Not_Delete_A_Published_Release()
        {
            var release = new Release { Id = 1 };

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromResult(release));

            await _vcsService.DiscardRelease(_owner, _repository, _tagName).ConfigureAwait(false);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().DeleteReleaseAsync(_owner, _repository, release.Id).ConfigureAwait(false);
            _logger.Received(1).Warning(Arg.Any<string>(), _tagName);
        }

        [Test]
        public async Task Should_Log_An_Error_On_Deleting_A_Release_For_Non_Existing_Tag()
        {
            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromException<Release>(_notFoundException));

            await _vcsService.DiscardRelease(_owner, _repository, _tagName).ConfigureAwait(false);

            await _vcsProvider.Received().GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceiveWithAnyArgs().DeleteReleaseAsync(_owner, _repository, default).ConfigureAwait(false);
            _logger.Received(1).Error(Arg.Any<string>(), _tagName);
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

            var result = await _vcsService.ExportReleases(_owner, _repository, null).ConfigureAwait(false);
            result.ShouldBeSameAs(releaseNotes);

            await _vcsProvider.DidNotReceive().GetReleaseAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).ConfigureAwait(false);
            await _vcsProvider.Received(1).GetReleasesAsync(_owner, _repository).ConfigureAwait(false);
            _logger.Received(1).Verbose(Arg.Any<string>(), _owner, _repository);
            _releaseNotesExporter.Received(1).ExportReleaseNotes(Arg.Any<IEnumerable<Release>>());
        }

        [Test]
        public async Task Should_Get_Release_Notes_For_A_Tag()
        {
            var release = new Release();
            var releaseNotes = "Release Notes";

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromResult(release));

            _releaseNotesExporter.ExportReleaseNotes(Arg.Any<IEnumerable<Release>>())
                .Returns(releaseNotes);

            var result = await _vcsService.ExportReleases(_owner, _repository, _tagName).ConfigureAwait(false);
            result.ShouldBeSameAs(releaseNotes);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            await _vcsProvider.DidNotReceive().GetReleasesAsync(Arg.Any<string>(), Arg.Any<string>()).ConfigureAwait(false);
            _logger.Received(1).Verbose(Arg.Any<string>(), _owner, _repository, _tagName);
            _releaseNotesExporter.Received(1).ExportReleaseNotes(Arg.Any<IEnumerable<Release>>());
        }

        [Test]
        public async Task Should_Get_Default_Release_Notes_For_Non_Existent_Tag()
        {
            var releaseNotes = "Release Notes";

            _vcsProvider.GetReleaseAsync(_owner, _repository, _tagName)
                .Returns(Task.FromException<Release>(_notFoundException));

            _releaseNotesExporter.ExportReleaseNotes(Arg.Any<IEnumerable<Release>>())
                .Returns(releaseNotes);

            var result = await _vcsService.ExportReleases(_owner, _repository, _tagName).ConfigureAwait(false);
            result.ShouldBeSameAs(releaseNotes);

            await _vcsProvider.Received(1).GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            _logger.Received(1).Error(Arg.Any<string>(), _tagName);
            _releaseNotesExporter.Received(1).ExportReleaseNotes(Arg.Any<IEnumerable<Release>>());
        }
    }
}