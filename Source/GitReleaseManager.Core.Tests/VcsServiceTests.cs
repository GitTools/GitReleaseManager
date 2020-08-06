using System.Collections.Generic;
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
using NotFoundException = GitReleaseManager.Core.Exceptions.NotFoundException;
using Release = GitReleaseManager.Core.Model.Release;

namespace GitReleaseManager.Core.Tests
{
    public class VcsServiceTests
    {
        private const string _owner = "owner";
        private const string _repository = "repository";
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

        [TestCase(null)]
        [TestCase(_tagName)]
        public async Task Should_Get_Release_Notes(string tagName)
        {
            var releaseNotes = "Release Notes";

            _releaseNotesExporter.ExportReleaseNotes(Arg.Any<IEnumerable<Release>>())
                .Returns(releaseNotes);

            var result = await _vcsService.ExportReleases(_owner, _repository, tagName).ConfigureAwait(false);
            result.ShouldBeSameAs(releaseNotes);

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
            _releaseNotesExporter.Received(1).ExportReleaseNotes(Arg.Any<IEnumerable<Release>>());
        }
    }
}