using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalTests;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Exceptions;
using GitReleaseManager.Core.Helpers;
using GitReleaseManager.Core.Model;
using GitReleaseManager.Core.Provider;
using GitReleaseManager.Core.ReleaseNotes;
using GitReleaseManager.Core.Templates;
using NSubstitute;
using NUnit.Framework;
using Serilog;

namespace GitReleaseManager.Tests
{
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
        public void SingularCommitsWithMilestoneDescription()
        {
            AcceptTest(1, CreateMilestone("2.4.2", "I am some awesome milestone description."), CreateIssue(5, "Feature"));
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
        public void CorrectlyUseFooterWhenEnabled()
        {
            var config = new Config();
            config.Create.IncludeFooter = true;
            config.Create.FooterHeading = "I am a header";
            config.Create.FooterContent = "I am content";

            AcceptTest(2, config, CreateIssue(6, "Bug"));
        }

        [Test]
        public void CorrectlyUseFooterWithMilestoneWhenEnabled()
        {
            var config = new Config();
            config.Create.IncludeFooter = true;
            config.Create.FooterHeading = "I am a header";
            config.Create.FooterContent = "I am a content with milestone {milestone}!";
            config.Create.MilestoneReplaceText = "{milestone}";

            AcceptTest(4, config, CreateIssue(78, "Feature"));
        }

        [Test]
        public void NoCommitsWrongIssueLabel()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(0, CreateIssue(1, "Test")));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidIssuesException>());
        }

        [Test]
        public void SomeCommitsWrongIssueLabel()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(5, CreateIssue(1, "Test")));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidIssuesException>());
        }

        [Test]
        public void NoCommitsMultipleWrongIssueLabel()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(0, CreateIssue(1, "Test"), CreateIssue(2, "Test")));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidIssuesException>());
            Assert.That((exception.InnerException as InvalidIssuesException).Errors.Count, Is.EqualTo(2));
        }

        [Test]
        public void SomeCommitsMultipleWrongIssueLabel()
        {
            var exception = Assert.Throws<AggregateException>(() => AcceptTest(5, CreateIssue(1, "Test"), CreateIssue(2, "Test"), CreateIssue(3, "Bob")));
            Assert.That(exception.InnerException, Is.Not.Null.And.TypeOf<InvalidIssuesException>());
            Assert.That((exception.InnerException as InvalidIssuesException).Errors.Count, Is.EqualTo(3));
        }

        [Test]
        public void CorrectlyExcludeIssues()
        {
            AcceptTest(5, CreateIssue(1, "Build"), CreateIssue(2, "Bug"));
            Assert.True(true); // Just to make sonarlint happy
        }

        private static void AcceptTest(int commits, params Issue[] issues)
        {
            AcceptTest(commits, null, null, issues);
            Assert.True(true); // Just to make sonarlint happy
        }

        private static void AcceptTest(int commits, Milestone milestone, params Issue[] issues)
        {
            AcceptTest(commits, null, milestone, issues);
            Assert.True(true); // Just to make sonarlint happy
        }

        private static void AcceptTest(int commits, Config config, params Issue[] issues)
        {
            AcceptTest(commits, config, null, issues);
            Assert.True(true); // Just to make sonarlint happy
        }

        private static void AcceptTest(int commits, Config config, Milestone milestone, params Issue[] issues)
        {
            var owner = "TestUser";
            var repository = "FakeRepository";
            var milestoneNumber = 1;
            milestone ??= CreateMilestone("1.2.3");

            var vcsService = new VcsServiceMock();
            var logger = Substitute.For<ILogger>();
            var fileSystem = Substitute.For<IFileSystem>();
            var currentDirectory = Environment.CurrentDirectory;
            var configuration = config ?? ConfigurationProvider.Provide(currentDirectory, fileSystem);

            vcsService.Milestones.Add(milestone);

            vcsService.NumberOfCommits = commits;

            foreach (var issue in issues)
            {
                vcsService.Issues.Add(issue);
            }

            var vcsProvider = Substitute.For<IVcsProvider>();
            vcsProvider.GetCommitsCountAsync(owner, repository, Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(vcsService.NumberOfCommits));

            vcsProvider.GetCommitsUrl(owner, repository, Arg.Any<string>(), Arg.Any<string>())
                .Returns(o => new GitHubProvider(null, null).GetCommitsUrl((string)o[0], (string)o[1], (string)o[2], (string)o[3]));

            vcsProvider.GetIssuesAsync(owner, repository, milestone, ItemStateFilter.Closed)
                .Returns(Task.FromResult((IEnumerable<Issue>)vcsService.Issues));

            vcsProvider.GetMilestonesAsync(owner, repository, Arg.Any<ItemStateFilter>())
                .Returns(Task.FromResult((IEnumerable<Milestone>)vcsService.Milestones));

            vcsProvider.GetMilestoneQueryString()
                .Returns("closed=1");

            var builder = new ReleaseNotesBuilder(vcsProvider, logger, fileSystem, configuration, new TemplateFactory(fileSystem, configuration, TemplateKind.Create));
            var notes = builder.BuildReleaseNotesAsync(owner, repository, milestone.Title, ReleaseTemplates.DEFAULT_NAME).Result;

            Approvals.Verify(notes);
        }

        private static Milestone CreateMilestone(string version, string description = null)
        {
            return new Milestone
            {
                Title = version,
                Description = description,
                PublicNumber = 1,
                InternalNumber = 123,
                HtmlUrl = "https://github.com/gep13/FakeRepository/issues?q=milestone%3A" + version,
                Version = new Version(version),
            };
        }

        private static Issue CreateIssue(int number, params string[] labels)
        {
            return new Issue
            {
                Number = number,
                Labels = labels.Select(l => new Label { Name = l }).ToList(),
                HtmlUrl = "http://example.com/" + number,
                Title = "Issue " + number,
            };
        }
    }
}