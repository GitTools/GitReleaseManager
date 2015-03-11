//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilderTests.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Tests
{
    using System;
    using System.Linq;
    using ApprovalTests;
    using GitHubReleaseManager.Configuration;
    using GitHubReleaseManager.Helpers;
    using NUnit.Framework;
    using Octokit;

    [TestFixture]
    public class ReleaseNotesBuilderTests
    {
        [Test]
        public void NoCommitsNoIssues()
        {
            AcceptTest(0);
        }

        [Test]
        public void NoCommitsSomeIssues()
        {
            AcceptTest(0, CreateIssue(1, "Bug"), CreateIssue(2, "Feature"), CreateIssue(3, "Improvement"));
        }

        [Test]
        public void SomeCommitsNoIssues()
        {
            AcceptTest(5);
        }

        [Test]
        public void SomeCommitsSomeIssues()
        {
            AcceptTest(5, CreateIssue(1, "Bug"), CreateIssue(2, "Feature"), CreateIssue(3, "Improvement"));
        }

        [Test]
        public void SingularCommitsNoIssues()
        {
            AcceptTest(1);
        }

        [Test]
        public void SingularCommitsSomeIssues()
        {
            AcceptTest(1, CreateIssue(1, "Bug"), CreateIssue(2, "Feature"), CreateIssue(3, "Improvement"));
        }

        [Test]
        public void SingularCommitsSingularIssues()
        {
            AcceptTest(1, CreateIssue(1, "Bug"));
        }

        [Test]
        public void NoCommitsSingularIssues()
        {
            AcceptTest(0, CreateIssue(1, "Bug"));
        }

        [Test]
        public void SomeCommitsSingularIssues()
        {
            AcceptTest(5, CreateIssue(1, "Bug"));
        }

        [Test]
        [ExpectedException(typeof(AggregateException))]
        public void NoCommitsWrongIssueLabel()
        {
            AcceptTest(0, CreateIssue(1, "Test"));
        }

        [Test]
        [ExpectedException(typeof(AggregateException))]
        public void SomeCommitsWrongIssueLabel()
        {
            AcceptTest(5, CreateIssue(1, "Test"));
        }

        private static void AcceptTest(int commits, params Issue[] issues)
        {
            var fakeClient = new FakeGitHubClient();
            var fileSystem = new FileSystem();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = ConfigurationProvider.Provide(currentDirectory, fileSystem);

            fakeClient.Milestones.Add(CreateMilestone("1.2.3"));

            fakeClient.NumberOfCommits = commits;

            foreach (var issue in issues)
            {
                fakeClient.Issues.Add(issue);
            }

            var builder = new ReleaseNotesBuilder(fakeClient, "TestUser", "FakeRepository", "1.2.3", configuration);
            var notes = builder.BuildReleaseNotes().Result;

            Approvals.Verify(notes);
        }

        private static Milestone CreateMilestone(string version)
        {
            return new Milestone
                {
                    Title = version,
                    Url = new Uri("https://github.com/Particular/FakeRepo/issues?q=milestone%3A" + version)
                };
        }

        private static Issue CreateIssue(int number, params string[] labels)
        {
            return new Issue
                {
                    Number = number,
                    Title = "Issue " + number,
                    HtmlUrl = new Uri("http://example.com/" + number),
                    Body = "Some issue",
                    Labels = labels.Select(x => new Label { Name = x }).ToArray(),
                };
        }
    }
}