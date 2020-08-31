// -----------------------------------------------------------------------
// <copyright file="EnsureTests.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Tests
{
    using System;
    using System.IO;
    using NUnit.Framework;
    using Shouldly;

    [TestFixture]
    public class EnsureTests
    {
        [Test]
        public void Should_Throw_Exception_When_String_Is_Null()
        {
            var paramName = "str";

            var ex = Should.Throw<ArgumentNullException>(() => Ensure.IsNotNullOrWhiteSpace(null, paramName));
            ex.ParamName.ShouldBe(paramName);
        }

        [TestCase("")]
        [TestCase(" ")]
        public void Should_Throw_Exception_When_String_Is_Whitespace(string str)
        {
            var paramName = nameof(str);

            var ex = Should.Throw<ArgumentException>(() => Ensure.IsNotNullOrWhiteSpace(str, paramName));
            ex.Message.ShouldContain("Value cannot be empty or white-space.");
            ex.ParamName.ShouldBe(paramName);
        }

        [Test]
        public void Should_Throw_Exception_When_File_Not_Exists()
        {
            var tempPath = Path.GetTempPath();
            var tempFile = "TempFile.txt";

            var path = Path.Combine(tempPath, tempFile);
            var message = "File does not exist";

            var ex = Should.Throw<FileNotFoundException>(() => Ensure.FileExists(path, message));
            ex.Message.ShouldBe(message);
            ex.FileName.ShouldBe(tempFile);
        }
    }
}