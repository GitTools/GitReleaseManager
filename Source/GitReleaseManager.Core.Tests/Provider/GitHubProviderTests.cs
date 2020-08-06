using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GitReleaseManager.Core.Provider;
using NSubstitute;
using NUnit.Framework;
using Octokit;
using Shouldly;
using Issue = GitReleaseManager.Core.Model.Issue;
using ItemStateFilter = GitReleaseManager.Core.Model.ItemStateFilter;

namespace GitReleaseManager.Core.Tests.Provider
{
    [TestFixture]
    public class GitHubProviderTests
    {
        private readonly string _owner = "owner";
        private readonly string _repository = "repository";
        private readonly int _milestoneNumber = 1;
        private readonly string _milestoneNumberString = "1";
        private readonly Octokit.ApiValidationException _apiValidationException = new Octokit.ApiValidationException();


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
        [TestCase("0.1.0", null, "https://github.com/owner/repository/commits/0.1.0")]
        [TestCase("0.5.0", "0.1.0", "https://github.com/owner/repository/compare/0.1.0...0.5.0")]
        public void Should_Get_A_Commits_Url(string milestoneTitle, string compareMilestoneTitle, string expectedResult)
        {
            var result = _gitHubProvider.GetCommitsUrl(_owner, _repository, milestoneTitle, compareMilestoneTitle);
            result.ShouldBe(expectedResult);
        }

        [TestCaseSource(nameof(GetCommitsUrl_TestCases))]
        public void Should_Throw_An_Exception_If_Parameter_Is_Invalid(string owner, string repository, string milestoneTitle, string paramName, Type expectedException)
        {
            var ex = Should.Throw(() => _gitHubProvider.GetCommitsUrl(owner, repository, milestoneTitle), expectedException);
            ex.Message.ShouldContain(paramName);
        }

        public static IEnumerable GetCommitsUrl_TestCases()
        {
            yield return new TestCaseData(null, null, null, "owner", typeof(ArgumentNullException));
            yield return new TestCaseData("", null, null, "owner", typeof(ArgumentException));
            yield return new TestCaseData(" ", null, null, "owner", typeof(ArgumentException));

            yield return new TestCaseData("owner", null, null, "repository", typeof(ArgumentNullException));
            yield return new TestCaseData("owner", "", null, "repository", typeof(ArgumentException));
            yield return new TestCaseData("owner", " ", null, "repository", typeof(ArgumentException));

            yield return new TestCaseData("owner", "repository", null, "milestoneTitle", typeof(ArgumentNullException));
            yield return new TestCaseData("owner", "repository", "", "milestoneTitle", typeof(ArgumentException));
            yield return new TestCaseData("owner", "repository", " ", "milestoneTitle", typeof(ArgumentException));
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
                .Returns(Task.FromException<IReadOnlyList<Octokit.Issue>>(_apiValidationException));

            var ex = await Should.ThrowAsync<ApiException>(() => _gitHubProvider.GetIssuesAsync(_owner, _repository, 1)).ConfigureAwait(false);
            ex.Message.ShouldBe(_apiValidationException.Message);
            ex.InnerException.ShouldBe(_apiValidationException);
        }
    }
}