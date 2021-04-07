using System;
using System.Collections;
using System.IO;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Helpers;
using GitReleaseManager.Core.Templates;
using NSubstitute;
using NUnit.Framework;
using Scriban;
using Shouldly;

namespace GitReleaseManager.Core.Tests.Templates
{
    [TestFixture]
    public class TemplateLoaderTests
    {
        public static IEnumerable PossibleScribanTestPaths
        {
            get
            {
                var basePath = Path.Combine(Environment.CurrentDirectory, ".templates");
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "index.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "index.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "default.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "index.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "index.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "default.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "index.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "index.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "default.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "index.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "index.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default.scriban"));
            }
        }

        public static IEnumerable PossibleScribanRelativePaths
        {
            get
            {
                var basePath = Path.Combine(Environment.CurrentDirectory, ".templates");
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "test.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "test.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "test.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "test.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "test.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "test.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "test.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "test.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "test.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "test.scriban"));
            }
        }

        public static IEnumerable ValidFilePathForResolvingResources
        {
            get
            {
                var basePath = Path.Combine(Environment.CurrentDirectory, ".templates");
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "index.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "index.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "create", "default.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "index.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "index.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default", "default.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "index.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "index.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default", "default.scriban"));
            }
        }

        public static IEnumerable InvalidFilePathsForResolvigResources
        {
            get
            {
                var basePath = Path.Combine(Environment.CurrentDirectory, ".templates");
                yield return new TestCaseData(Path.Combine(basePath, "create", "index.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "index.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "create", "default.scriban"));
                yield return new TestCaseData(Path.Combine(basePath, "default.sbn"));
                yield return new TestCaseData(Path.Combine(basePath, "default.scriban"));
            }
        }

        public static IEnumerable ResourceTemplateLoading
        {
            get
            {
                yield return new TestCaseData("default/create/footer", ReleaseTemplates.DEFAULT_CREATE_FOOTER);
                yield return new TestCaseData("default/create/index", ReleaseTemplates.DEFAULT_INDEX);
                yield return new TestCaseData("default/create/issue-details", ReleaseTemplates.DEFAULT_ISSUE__DETAILS);
                yield return new TestCaseData("default/create/issue-note", ReleaseTemplates.DEFAULT_ISSUE__NOTE);
                yield return new TestCaseData("default/create/issues", ReleaseTemplates.DEFAULT_ISSUES);
                yield return new TestCaseData("default/create/milestone", ReleaseTemplates.DEFAULT_MILESTONE);
                yield return new TestCaseData("default/create/release-info", ReleaseTemplates.DEFAULT_RELEASE__INFO);
            }
        }

        [Test]
        public void Should_GetDefaultResourcePath()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            var config = new Config();
            var loader = new TemplateLoader(config, fileSystem, TemplateKind.Create);

            var result = loader.GetPath(null, default, "default");

            result.ShouldBe(ReleaseTemplates.RESOURCE_PREFIX + "default/create/index");
        }

        [Test]
        public void Should_GetRelativeResourcePath()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            var config = new Config();
            var loader = new TemplateLoader(config, fileSystem, TemplateKind.Create);
            var templateContext = new TemplateContext();
            templateContext.PushSourceFile(ReleaseTemplates.RESOURCE_PREFIX + "default/create/index");

            var result = loader.GetPath(templateContext, default, "release-info");

            result.ShouldBe(ReleaseTemplates.RESOURCE_PREFIX + "default/create/release-info");
        }

        [TestCase("index.scriban")]
        [TestCase("index.sbn")]
        public void Should_GetFullIndexPathWhenFileExists(string expectedFileName)
        {
            var expectedFile = Path.Combine(Environment.CurrentDirectory, ".templates", "default", "create", expectedFileName);
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(null, default, "default");

            result.ShouldBe(expectedFile);
        }

        [TestCase("test.scriban")]
        [TestCase("grm.sbn")]
        public void Should_GetRelativeFileWhenExists(string expectedFileName)
        {
            var expectedFile = Path.Combine(Environment.CurrentDirectory, ".templates", "default", "create", expectedFileName);
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);
            var context = new TemplateContext();
            context.PushSourceFile(Path.Combine(Environment.CurrentDirectory, ".templates", "default", "create", "index.sbn"));

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(context, default, Path.GetFileNameWithoutExtension(expectedFileName));

            result.ShouldBe(expectedFile);
        }

        [TestCase("txt")]
        [TestCase("md")]
        [TestCase("html")]
        public void Should_GetFullIndexPathWhenExtensionIsUsedAndFileExists(string extension)
        {
            var expectedFile = Path.Combine(Environment.CurrentDirectory, ".templates", "default", "create", "index." + extension);
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(null, default, "default." + extension);

            result.ShouldBe(expectedFile);
        }

        [TestCase("txt")]
        [TestCase("md")]
        [TestCase("html")]
        public void Should_GetRelativeWhenExtensionIsUsedAndFileWhenExists(string extension)
        {
            var expectedFile = Path.Combine(Environment.CurrentDirectory, ".templates", "default", "create", "test-file." + extension);
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);
            var context = new TemplateContext();
            context.PushSourceFile(Path.Combine(Environment.CurrentDirectory, ".templates", "default", "create", "index.sbn"));

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(context, default, "test-file." + extension);

            result.ShouldBe(expectedFile);
        }

        [Test]
        public void Should_GetFilePathWhenPreviousSourceWasResourcefile()
        {
            var expectedFile = Path.Combine(Environment.CurrentDirectory, ".templates", "default", "create", "releases.sbn");
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);
            var context = new TemplateContext();
            context.PushSourceFile(ReleaseTemplates.RESOURCE_PREFIX + "default/create/full");

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(context, default, "releases");

            result.ShouldBe(expectedFile);
        }

        [TestCaseSource(nameof(ValidFilePathForResolvingResources))]
        public void Should_GetResourcePathWhenPreviousSourceWasFile(string sourcePath)
        {
            var expected = ReleaseTemplates.RESOURCE_PREFIX + "default/create/note";
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            var context = new TemplateContext();
            context.PushSourceFile(sourcePath);

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(context, default, "note");

            result.ShouldBe(expected);
        }

        [TestCaseSource(nameof(InvalidFilePathsForResolvigResources))]
        public void Should_ThrowFileNotFoundExceptionOnInvalidFilePathsForResourceFallback(string sourcePath)
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            var context = new TemplateContext();
            context.PushSourceFile(sourcePath);

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);

            Should.Throw<FileNotFoundException>(() => loader.GetPath(context, default, "test"));
        }

        [Test]
        public void Should_GetExistingFileExtensionAsPreviousFile()
        {
            var expectedFile = Path.Combine(Environment.CurrentDirectory, ".templates", "xml", "create", "release.xml");
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);
            var context = new TemplateContext();
            context.PushSourceFile(Path.Combine(Environment.CurrentDirectory, ".templates", "xml", "create", "index.xml"));

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(context, default, "release");

            result.ShouldBe(expectedFile);
        }

        [TestCaseSource(nameof(PossibleScribanTestPaths))]
        public void Should_GetExpectedSourcePaths(string expectedFile)
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(null, default, "default");

            result.ShouldBe(expectedFile);
        }

        [TestCaseSource(nameof(PossibleScribanRelativePaths))]
        public void Should_GetExpectedRelativePathsWhenPreviousIsResource(string expectedFile)
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);
            var context = new TemplateContext();
            context.PushSourceFile(ReleaseTemplates.RESOURCE_PREFIX + "default/create/index");

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(context, default, "test");

            result.ShouldBe(expectedFile);
        }

        [Test]
        public void Should_GetValidPathFromAbsolutePath()
        {
            var expectedFile = Path.Combine(Environment.CurrentDirectory, "test-file.md");
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(null, default, expectedFile);

            result.ShouldBe(expectedFile);
        }

        [TestCase("sbn")]
        [TestCase("scriban")]
        public void Should_GetValidPathFromAbsolutePathWithoutExtension(string extension)
        {
            var expectedFile = Path.Combine(Environment.CurrentDirectory, "test-file." + extension);
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.Exists(Arg.Any<string>()).Returns(false);
            fileSystem.Exists(expectedFile).Returns(true);

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.GetPath(null, default, Path.Combine(Environment.CurrentDirectory, "test-file"));

            result.ShouldBe(expectedFile);
        }

        [TestCaseSource(nameof(ResourceTemplateLoading))]
        public void Should_LoadReleaseTemplateFromResource(string key, string expected)
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.Load(null, default, ReleaseTemplates.RESOURCE_PREFIX + key);

            result.ShouldBe(expected);
        }

        [Test]
        public void Should_ThrowExceptionOnInvalidResource()
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);

            Should.Throw<ArgumentOutOfRangeException>(() => loader.Load(null, default, ReleaseTemplates.RESOURCE_PREFIX + "invalid"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public void Should_ReturnEmptyTextWhenPathIsEmpty(string path)
        {
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.Load(null, default, path);

            result.ShouldBeEmpty();
        }

        [Test]
        public void Should_LoadAllTextFromFilePath()
        {
            var expected = "I WAS Loaded!!!";
            var testPath = Path.Combine(Environment.CurrentDirectory, "test.sbn");
            var fileSystem = Substitute.For<IFileSystem>();
            fileSystem.ResolvePath(Arg.Any<string>()).Returns(s => Path.Combine(Environment.CurrentDirectory, s.Arg<string>()));
            fileSystem.ReadAllText(testPath).Returns(expected);

            var loader = new TemplateLoader(new Config(), fileSystem, TemplateKind.Create);
            var result = loader.Load(null, default, testPath);

            result.ShouldBe(expected);
        }
    }
}