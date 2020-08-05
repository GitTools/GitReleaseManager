using System;
using System.Collections;
using GitReleaseManager.Core.Provider;
using NUnit.Framework;
using Shouldly;

namespace GitReleaseManager.Core.Tests.Provider
{
    [TestFixture]
    public class GitHubProviderTests
    {
        [TestCase("0.1.0", null, "https://github.com/owner/repo/commits/0.1.0")]
        [TestCase("0.5.0", "0.1.0", "https://github.com/owner/repo/compare/0.1.0...0.5.0")]
        public void Should_Get_A_Commits_Url(string milestoneTitle, string compareMilestoneTitle, string expectedResult)
        {
            var gitHubProvider = new GitHubProvider();

            var result = gitHubProvider.GetCommitsUrl("owner", "repo", milestoneTitle, compareMilestoneTitle);
            result.ShouldBe(expectedResult);
        }

        [TestCaseSource(nameof(GetCommitsUrl_TestCases))]
        public void Should_Throw_An_Exception_If_Parameter_Is_Invalid(string owner, string repository, string milestoneTitle, string paramName, Type expectedException)
        {
            var gitHubProvider = new GitHubProvider();

            var ex = Should.Throw(() => gitHubProvider.GetCommitsUrl(owner, repository, milestoneTitle), expectedException);
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
    }
}