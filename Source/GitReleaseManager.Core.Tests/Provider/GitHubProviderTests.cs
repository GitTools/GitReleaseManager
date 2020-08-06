using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GitReleaseManager.Core.Provider;
using NSubstitute;
using NUnit.Framework;
using Octokit;
using Shouldly;
using Issue = GitReleaseManager.Core.Model.Issue;
using ItemStateFilter = GitReleaseManager.Core.Model.ItemStateFilter;
using Milestone = GitReleaseManager.Core.Model.Milestone;
using NotFoundException = GitReleaseManager.Core.Exceptions.NotFoundException;
using Release = GitReleaseManager.Core.Model.Release;

namespace GitReleaseManager.Core.Tests.Provider
{
    [TestFixture]
    public class GitHubProviderTests
    {
        private readonly string _owner = "owner";
        private readonly string _repository = "repository";
        private readonly string _base = "0.1.0";
        private readonly string _head = "0.5.0";
        private readonly int _milestoneNumber = 1;
        private readonly string _milestoneNumberString = "1";
        private readonly string _tagName = "0.1.0";
        private readonly Exception _exception = new Exception("API Error");
        private readonly Octokit.NotFoundException _notFoundException = new Octokit.NotFoundException("NotFound", HttpStatusCode.NotFound);

        private IMapper _mapper;
        private IGitHubClient _gitHubClient;
        private GitHubProvider _gitHubProvider;

        [SetUp]
        public void Setup()
        {
            _mapper = Substitute.For<IMapper>();
            _gitHubClient = Substitute.For<IGitHubClient>();
            _gitHubProvider = new GitHubProvider(_gitHubClient, _mapper);
        }

        // Commits
        [Test]
        public async Task Should_Get_Commits_Count()
        {
            var commitsCount = 12;

            _gitHubClient.Repository.Commit.Compare(_owner, _repository, _base, _head)
                .Returns(Task.FromResult(new CompareResult(null, null, null, null, null, null, null, null, commitsCount, 0, 0, null, null)));

            var result = await _gitHubProvider.GetCommitsCount(_owner, _repository, _base, _head).ConfigureAwait(false);
            result.ShouldBe(commitsCount);

            await _gitHubClient.Repository.Commit.Received(1).Compare(_owner, _repository, _base, _head).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Get_Commits_Count_Zero_If_No_Commits_Found()
        {
            _gitHubClient.Repository.Commit.Compare(_owner, _repository, _base, _head)
                .Returns(Task.FromException<CompareResult>(_notFoundException));

            var result = await _gitHubProvider.GetCommitsCount(_owner, _repository, _base, _head).ConfigureAwait(false);
            result.ShouldBe(0);

            await _gitHubClient.Repository.Commit.Received(1).Compare(_owner, _repository, _base, _head).ConfigureAwait(false);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Commits_Count()
        {
            _gitHubClient.Repository.Commit.Compare(_owner, _repository, _base, _head)
                .Returns(Task.FromException<CompareResult>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetCommitsCount(_owner, _repository, _base, _head)).ConfigureAwait(false);
            ex.Message.ShouldContain(_exception.Message);
            ex.InnerException.ShouldBeSameAs(_exception);
        }

        [TestCase("0.1.0", null, "https://github.com/owner/repository/commits/0.1.0")]
        [TestCase("0.5.0", "0.1.0", "https://github.com/owner/repository/compare/0.1.0...0.5.0")]
        public void Should_Get_A_Commits_Url(string head, string @base, string expectedResult)
        {
            var result = _gitHubProvider.GetCommitsUrl(_owner, _repository, head, @base);
            result.ShouldBe(expectedResult);
        }

        [TestCaseSource(nameof(GetCommitsUrl_TestCases))]
        public void Should_Throw_An_Exception_If_Parameter_Is_Invalid(string owner, string repository, string head, string paramName, Type expectedException)
        {
            var ex = Should.Throw(() => _gitHubProvider.GetCommitsUrl(owner, repository, head), expectedException);
            ex.Message.ShouldContain(paramName);
        }

        public static IEnumerable GetCommitsUrl_TestCases()
        {
            var typeArgumentException = typeof(ArgumentException);
            var typeArgumentNullException = typeof(ArgumentNullException);

            yield return new TestCaseData(null, null, null, "owner", typeArgumentNullException);
            yield return new TestCaseData("", null, null, "owner", typeArgumentException);
            yield return new TestCaseData(" ", null, null, "owner", typeArgumentException);

            yield return new TestCaseData("owner", null, null, "repository", typeArgumentNullException);
            yield return new TestCaseData("owner", "", null, "repository", typeArgumentException);
            yield return new TestCaseData("owner", " ", null, "repository", typeArgumentException);

            yield return new TestCaseData("owner", "repository", null, "head", typeArgumentNullException);
            yield return new TestCaseData("owner", "repository", "", "head", typeArgumentException);
            yield return new TestCaseData("owner", "repository", " ", "head", typeArgumentException);
        }

        // Issues
        [TestCase(ItemStateFilter.Open)]
        [TestCase(ItemStateFilter.Closed)]
        [TestCase(ItemStateFilter.All)]
        public async Task Should_Get_Issues_For_A_Milestone(ItemStateFilter itemStateFilter)
        {
            var issues = new List<Issue>();

            _gitHubClient.Issue.GetAllForRepository(_owner, _repository, Arg.Any<RepositoryIssueRequest>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Issue>)new List<Octokit.Issue>()));

            _mapper.Map<IEnumerable<Issue>>(Arg.Any<object>())
                .Returns(issues);

            var result = await _gitHubProvider.GetIssuesAsync(_owner, _repository, _milestoneNumber, itemStateFilter).ConfigureAwait(false);
            result.ShouldBeSameAs(issues);

            await _gitHubClient.Issue.Received(1).GetAllForRepository(_owner, _repository, Arg.Is<RepositoryIssueRequest>(o =>
                    o.Milestone == _milestoneNumberString &&
                    o.State == (Octokit.ItemStateFilter)itemStateFilter)).ConfigureAwait(false);

            _mapper.ReceivedWithAnyArgs(1).Map<IEnumerable<Issue>>(default);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Issues_For_Non_Existing_Milestone()
        {
            _gitHubClient.Issue.GetAllForRepository(_owner, _repository, Arg.Any<RepositoryIssueRequest>())
                .Returns(Task.FromException<IReadOnlyList<Octokit.Issue>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetIssuesAsync(_owner, _repository, 1)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        // Milestones
        [TestCase(ItemStateFilter.Open)]
        [TestCase(ItemStateFilter.Closed)]
        [TestCase(ItemStateFilter.All)]
        public async Task Should_Get_Milestone(ItemStateFilter itemStateFilter)
        {
            var milestones = new List<Milestone>();

            _gitHubClient.Issue.Milestone.GetAllForRepository(_owner, _repository, Arg.Any<MilestoneRequest>())
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Milestone>)new List<Octokit.Milestone>()));

            _mapper.Map<IEnumerable<Milestone>>(Arg.Any<object>())
                .Returns(milestones);

            var result = await _gitHubProvider.GetMilestonesAsync(_owner, _repository, itemStateFilter).ConfigureAwait(false);
            result.ShouldBeSameAs(milestones);

            await _gitHubClient.Issue.Milestone.Received(1).GetAllForRepository(_owner, _repository, Arg.Is<MilestoneRequest>(o =>
                o.State == (Octokit.ItemStateFilter)itemStateFilter)).ConfigureAwait(false);

            _mapper.ReceivedWithAnyArgs(1).Map<IEnumerable<Milestone>>(default);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Milestone()
        {
            _gitHubClient.Issue.Milestone.GetAllForRepository(_owner, _repository, Arg.Any<MilestoneRequest>())
                .Returns(Task.FromException<IReadOnlyList<Octokit.Milestone>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetMilestonesAsync(_owner, _repository)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        // Releases
        [Test]
        public async Task Should_Get_A_Release()
        {
            var release = new Release();

            _gitHubClient.Repository.Release.Get(_owner, _repository, _tagName)
                .Returns(Task.FromResult(new Octokit.Release()));

            _mapper.Map<Release>(Arg.Any<object>())
                .Returns(release);

            var result = await _gitHubProvider.GetReleaseAsync(_owner, _repository, _tagName).ConfigureAwait(false);
            result.ShouldBeSameAs(release);

            await _gitHubClient.Repository.Release.Received(1).Get(_owner, _repository, _tagName).ConfigureAwait(false);
            _mapper.Received(1).Map<Release>(Arg.Any<object>());
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Release_For_Non_Existing_Tag()
        {
            _gitHubClient.Repository.Release.Get(_owner, _repository, _tagName)
                .Returns(Task.FromException<Octokit.Release>(_notFoundException));

            var ex = await Should.ThrowAsync<NotFoundException>(() => _gitHubProvider.GetReleaseAsync(_owner, _repository, _tagName)).ConfigureAwait(false);
            ex.Message.ShouldBe(_notFoundException.Message);
            ex.InnerException.ShouldBe(_notFoundException);
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Release()
        {
            _gitHubClient.Repository.Release.Get(_owner, _repository, _tagName)
                .Returns(Task.FromException<Octokit.Release>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetReleaseAsync(_owner, _repository, _tagName)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }

        [Test]
        public async Task Should_Get_Releases()
        {
            var releases = new List<Release>();

            _gitHubClient.Repository.Release.GetAll(_owner, _repository)
                .Returns(Task.FromResult((IReadOnlyList<Octokit.Release>)new List<Octokit.Release>()));

            _mapper.Map<IEnumerable<Release>>(Arg.Any<object>())
                .Returns(releases);

            var result = await _gitHubProvider.GetReleasesAsync(_owner, _repository).ConfigureAwait(false);
            result.ShouldBeSameAs(releases);

            await _gitHubClient.Repository.Release.Received(1).GetAll(_owner, _repository).ConfigureAwait(false);
            _mapper.Received(1).Map<IEnumerable<Release>>(Arg.Any<object>());
        }

        [Test]
        public async Task Should_Throw_An_Exception_On_Getting_Releases()
        {
            _gitHubClient.Repository.Release.GetAll(_owner, _repository)
                .Returns(Task.FromException<IReadOnlyList<Octokit.Release>>(_exception));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetReleasesAsync(_owner, _repository)).ConfigureAwait(false);
            ex.Message.ShouldBe(_exception.Message);
            ex.InnerException.ShouldBe(_exception);
        }
    }
}