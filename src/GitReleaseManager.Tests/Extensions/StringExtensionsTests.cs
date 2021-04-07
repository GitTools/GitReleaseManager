using System.Collections.Generic;
using GitReleaseManager.Core.Extensions;
using NUnit.Framework;

namespace GitReleaseManager.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void Should_Replace_Variables_In_String()
        {
            const string expected = "This test was created by AdmiringWorm for GitReleaseManager.";
            const string textToTest = "This test was created by {User} for {Program}.";
            var variables = new { User = "AdmiringWorm", Program = "GitReleaseManager" };

            var actual = textToTest.ReplaceTemplate(variables);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Should_Replace_Variables_In_String_When_Case_Differs()
        {
            const string expected = "This test was created by AdmiringWorm for GitReleaseManager.";
            const string textToTest = "This test was created by {USeR} for {Program}.";
            var variables = new { User = "AdmiringWorm", ProgrAM = "GitReleaseManager" };

            var actual = textToTest.ReplaceTemplate(variables);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Should_Replace_Variables_When_Dictionary_Is_Used()
        {
            const string expected = "This test was created by AdmiringWorm for GitReleaseManager.";
            const string textToTest = "This test was created by {User} for {Program}.";
            var variables = new Dictionary<string, object>
            {
                { "User", "AdmiringWorm" },
                { "Program", "GitReleaseManager" },
            };

            var actual = textToTest.ReplaceTemplate(variables);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}