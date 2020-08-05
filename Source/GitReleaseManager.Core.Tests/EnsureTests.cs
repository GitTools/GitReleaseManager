using System;
using NUnit.Framework;
using Shouldly;

namespace GitReleaseManager.Core.Tests
{
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

        [Test]
        public void Should_Throw_Exception_When_String_Is_Whitespace([Values("", " ")] string str)
        {
            var paramName = nameof(str);

            var ex = Should.Throw<ArgumentException>(() => Ensure.IsNotNullOrWhiteSpace(str, paramName));
            ex.Message.ShouldContain("Value cannot be empty or white-space.");
            ex.ParamName.ShouldBe(paramName);
        }
    }
}