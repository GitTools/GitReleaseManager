using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GitReleaseManager.Core;
using GitReleaseManager.Core.Provider;
using NGitLab;
using NUnit.Framework;
using Serilog;
using Shouldly;
using Issue = GitReleaseManager.Core.Model.Issue;
using Milestone = GitReleaseManager.Core.Model.Milestone;

namespace GitReleaseManager.IntegrationTests
{
    [TestFixture]
    [Explicit]
    public class GitLabProviderIntegrationTests
    {
        private const string OWNER = "gep13";
        private const string REPOSITORY = "grm-test";
        private const bool SKIP_PRERELEASES = false;

        private GitLabProvider _gitLabProvider;
        private IGitLabClient _gitLabClient;
        private IMapper _mapper;
#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
        private ILogger _logger;
#pragma warning restore NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

        private string _token;
        private string _releaseBaseTag;
        private string _releaseHeadTag;
        private Issue _issue;
        private Milestone _milestone;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _token = Environment.GetEnvironmentVariable("GITTOOLS_GITLAB_TOKEN");

            if (string.IsNullOrWhiteSpace(_token))
            {
                Assert.Inconclusive("Unable to locate credentials for accessing GitLab API");
            }

            _mapper = AutoMapperConfiguration.Configure();
            _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            _gitLabClient = new GitLabClient("https://gitlab.com", _token);
            _gitLabProvider = new GitLabProvider(_gitLabClient, _mapper, _logger);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            (_logger as IDisposable)?.Dispose();
        }

        [Test]
        [Order(1)]
        public async Task Should_Get_Milestones()
        {
            var result = await _gitLabProvider.GetMilestonesAsync(OWNER, REPOSITORY).ConfigureAwait(false);
            result.Count().ShouldBeGreaterThan(0);

            _milestone = result.OrderByDescending(m => m.PublicNumber).First();
        }

        [Test]
        [Order(2)]
        public async Task Should_Get_Issues()
        {
            var result = await _gitLabProvider.GetIssuesAsync(OWNER, REPOSITORY, _milestone).ConfigureAwait(false);
            result.Count().ShouldBeGreaterThan(0);

            _issue = result.First();
        }

        [Test]
        [Order(3)]
        public async Task Should_Get_Issue_Comments()
        {
            var result = await _gitLabProvider.GetIssueCommentsAsync(OWNER, REPOSITORY, _issue).ConfigureAwait(false);
            result.Count().ShouldBeGreaterThan(0);
        }

        [Test]
        [Order(4)]
        public async Task Should_Get_Releases()
        {
            var result = await _gitLabProvider.GetReleasesAsync(OWNER, REPOSITORY, SKIP_PRERELEASES).ConfigureAwait(false);
            result.Count().ShouldBeGreaterThan(0);

            var orderedReleases = result.OrderByDescending(r => r.Id).ToList();

            _releaseBaseTag = orderedReleases[1].TagName;
            _releaseHeadTag = orderedReleases[0].TagName;
        }

        [Test]
        [Order(5)]
        public async Task Should_Get_Commits_Count()
        {
            // TODO: This is waiting on a PR being merged...
            // https://github.com/ubisoft/NGitLab/pull/444
            // Once it is, we might be able to implement what is necessary here.
            // var result = await _gitLabProvider.GetCommitsCountAsync(OWNER, REPOSITORY, _releaseBaseTag, _releaseHeadTag).ConfigureAwait(false);
            // result.ShouldBeGreaterThan(0);
        }
    }
}