//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesExporterTests.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
    using System;
    using System.Text;
    using ApprovalTests;
    using GitReleaseManager.Core;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using GitReleaseManager.Core.Model;
    using NUnit.Framework;

    [TestFixture]
    public class ReleaseNotesExporterTests
    {
        private FileSystem _fileSystem = new FileSystem();
        private string _currentDirectory = Environment.CurrentDirectory;

        [Test]
        public void NoReleases()
        {
            var configuration = ConfigurationProvider.Provide(_currentDirectory, _fileSystem);
            AcceptTest(configuration);
        }

        [Test]
        public void SingleRelease()
        {
            var configuration = ConfigurationProvider.Provide(_currentDirectory, _fileSystem);
            AcceptTest(configuration, CreateRelease(1, new DateTime(2015, 3, 12), "0.1.0"));
        }

        [Test]
        public void SingleReleaseExcludeCreatedDateInTitle()
        {
            var configuration = ConfigurationProvider.Provide(_currentDirectory, _fileSystem);
            configuration.Export.IncludeCreatedDateInTitle = false;

            AcceptTest(configuration, CreateRelease(1, new DateTime(2015, 3, 12), "0.1.0"));
        }

        [Test]
        public void SingleReleaseExcludeRegexRemoval()
        {
            var configuration = ConfigurationProvider.Provide(_currentDirectory, _fileSystem);
            configuration.Export.PerformRegexRemoval = false;

            AcceptTest(configuration, CreateRelease(1, new DateTime(2015, 3, 12), "0.1.0"));
        }

        private static void AcceptTest(Config configuration, params Release[] releases)
        {
            var fakeClient = new FakeGitHubClient();

            foreach (var release in releases)
            {
                fakeClient.Releases.Add(release);
            }

            var builder = new ReleaseNotesExporter(fakeClient, configuration, "bob", "repo");
            var notes = builder.ExportReleaseNotes(null).Result;

            Approvals.Verify(notes);
        }

        private static Release CreateRelease(int id, DateTime createdDateTime, string milestone)
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