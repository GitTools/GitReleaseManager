using System;
using System.Text;
using ApprovalTests;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Helpers;
using GitReleaseManager.Core.Model;
using GitReleaseManager.Core.ReleaseNotes;
using NSubstitute;
using NUnit.Framework;
using Serilog;

namespace GitReleaseManager.Tests
{
    [TestFixture]
    public class ReleaseNotesExporterTests
    {
        private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();
        private readonly string _currentDirectory = Environment.CurrentDirectory;

        [Test]
        public void NoReleases()
        {
            var configuration = ConfigurationProvider.Provide(_currentDirectory, _fileSystem);
            AcceptTest(configuration);
            Assert.That(true, Is.True); // Just to make sonarlint happy
        }

        [Test]
        public void SingleRelease()
        {
            var configuration = ConfigurationProvider.Provide(_currentDirectory, _fileSystem);
            AcceptTest(configuration, CreateRelease(new DateTime(2015, 3, 12), "0.1.0"));
            Assert.That(true, Is.True); // Just to make sonarlint happy
        }

        [Test]
        [NonParallelizable]
        public void SingleReleaseExcludeCreatedDateInTitle()
        {
            var configuration = ConfigurationProvider.Provide(_currentDirectory, _fileSystem);
            configuration.Export.IncludeCreatedDateInTitle = false;

            AcceptTest(configuration, CreateRelease(new DateTime(2015, 3, 12), "0.1.0"));
            Assert.That(true, Is.True); // Just to make sonarlint happy
        }

        [Test]
        [NonParallelizable]
        public void SingleReleaseExcludeRegexRemoval()
        {
            var configuration = ConfigurationProvider.Provide(_currentDirectory, _fileSystem);
            configuration.Export.PerformRegexRemoval = false;

            AcceptTest(configuration, CreateRelease(new DateTime(2015, 3, 12), "0.1.0"));
            Assert.That(true, Is.True); // Just to make sonarlint happy
        }

        private static void AcceptTest(Config configuration, params Release[] releases)
        {
            var logger = Substitute.For<ILogger>();
            var builder = new ReleaseNotesExporter(logger, configuration.Export);
            var notes = builder.ExportReleaseNotes(releases);

            Approvals.Verify(notes);
        }

        private static Release CreateRelease(DateTime createdDateTime, string milestone)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("As part of this release we had [3 issues](https://github.com/FakeRepository/issues/issues?milestone=0&state=closed) closed.");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("__Bug__");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("- [__#1__](http://example.com/1) Issue 1");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("__Feature__");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("- [__#2__](http://example.com/2) Issue 2");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("__Improvement__");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("- [__#3__](http://example.com/3) Issue 3");

            return new Release { TagName = milestone, Body = stringBuilder.ToString(), CreatedAt = createdDateTime };
        }
    }
}