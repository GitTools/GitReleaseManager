//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilderTests.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

using NSubstitute;
using Serilog;

namespace GitReleaseManager.Tests
{
    using System;
    using System.Linq;
    using ApprovalTests;
    using GitReleaseManager.Core;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using GitReleaseManager.Core.Model;
    using NUnit.Framework;

    [TestFixture]
    public class ReleaseNotesBuilderTests
    {
        [Test]
        public void NoCommitsNoIssues()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(0));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void NoCommitsSomeIssues()
        {
            AcceptTest(0, CreateIssue(1, "Bug"), CreateIssue(2, "Feature"), CreateIssue(3, "Improvement"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void SomeCommitsNoIssues()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(5));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void SomeCommitsSomeIssues()
        {
            AcceptTest(5, CreateIssue(1, "Bug"), CreateIssue(2, "Feature"), CreateIssue(3, "Improvement"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void SingularCommitsNoIssues()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(1));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void SingularCommitsSomeIssues()
        {
            AcceptTest(1, CreateIssue(1, "Bug"), CreateIssue(2, "Feature"), CreateIssue(3, "Improvement"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void SingularCommitsSingularIssues()
        {
            AcceptTest(1, CreateIssue(1, "Bug"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void NoCommitsSingularIssues()
        {
            AcceptTest(0, CreateIssue(1, "Bug"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void SomeCommitsSingularIssues()
        {
            AcceptTest(5, CreateIssue(1, "Bug"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void SingularCommitsWithHeaderLabelAlias()
        {
            var config = new Config();
            config.LabelAliases.Add(new LabelAlias
            {
                Name = "Bug",
                Header = "Foo",
            });

            AcceptTest(1, config, CreateIssue(1, "Bug"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void SomeCommitsWithPluralizedLabelAlias()
        {
            var config = new Config();
            config.LabelAliases.Add(new LabelAlias
            {
                Name = "Help Wanted",
                Plural = "Bar",
            });

            AcceptTest(5, config, CreateIssue(1, "Help Wanted"), CreateIssue(2, "Help Wanted"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void SomeCommitsWithoutPluralizedLabelAlias()
        {
            AcceptTest(5, CreateIssue(1, "Help Wanted"), CreateIssue(2, "Help Wanted"));
            Assert.True(true); // Just to make sonarlint happy
        }

        [Test]
        public void NoCommitsWrongIssueLabel()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(0, CreateIssue(1, "Test")));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void SomeCommitsWrongIssueLabel()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(5, CreateIssue(1, "Test")));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void CorrectlyExcludeIssues()
        {
            AcceptTest(5, CreateIssue(1, "Build"), CreateIssue(2, "Bug"));
            Assert.True(true); // Just to make sonarlint happy
        }

        private static void AcceptTest(int commits, params Issue[] issues)
        {
            AcceptTest(commits, null, issues);
            Assert.True(true); // Just to make sonarlint happy
        }

        private static void AcceptTest(int commits, Config config, params Issue[] issues)
        {
            var fakeClient = new FakeGitHubClient();
            var logger = Substitute.For<ILogger>();
            var fileSystem = new FileSystem();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = config ?? ConfigurationProvider.Provide(currentDirectory, fileSystem);

            fakeClient.Milestones.Add(CreateMilestone("1.2.3"));

            fakeClient.NumberOfCommits = commits;

            foreach (var issue in issues)
            {
                fakeClient.Issues.Add(issue);
            }

            var builder = new ReleaseNotesBuilder(fakeClient, logger, "TestUser", "FakeRepository", "1.2.3", configuration);
            var notes = builder.BuildReleaseNotes().Result;

            Approvals.Verify(notes);
        }

        private static Milestone CreateMilestone(string version)
        {
            return new Milestone
            {
                Title = version,
                HtmlUrl = "https://github.com/gep13/FakeRepository/issues?q=milestone%3A" + version,
                Version = new Version(version),
            };
        }

        private static Issue CreateIssue(int number, params string[] labels)
        {
            return new Issue
            {
                Number = number.ToString(),
                Labels = labels.Select(l => new Label { Name = l }).ToList(),
                HtmlUrl = "http://example.com/" + number,
                Title = "Issue " + number,
            };
        }
    }
}