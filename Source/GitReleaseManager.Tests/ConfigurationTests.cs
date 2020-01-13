//-----------------------------------------------------------------------
// <copyright file="ConfigurationTests.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
    using System;
    using System.IO;
    using System.Text;
    using GitReleaseManager.Core.Configuration;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationTests
    {
        [Test]
        public void Should_Read_Label_Aliases()
        {
            // Given
            var text = Resources.Default_Configuration_Yaml;
            using (var stringReader = new StringReader(text))
            {
                // When
                var config = ConfigSerializer.Read(stringReader);

                // Then
                Assert.AreEqual(2, config.LabelAliases.Count);
                Assert.AreEqual("Bug", config.LabelAliases[0].Name);
                Assert.AreEqual("Foo", config.LabelAliases[0].Header);
                Assert.AreEqual("Bar", config.LabelAliases[0].Plural);
                Assert.AreEqual("Improvement", config.LabelAliases[1].Name);
                Assert.AreEqual("Baz", config.LabelAliases[1].Header);
                Assert.AreEqual("Qux", config.LabelAliases[1].Plural);
            }
        }

        [Test]
        public void Should_Write_Default_Boolean_Values()
        {
            // Given
            var config = new Config();
            config.Create.IncludeFooter = false; // Just to be sure it is a false value

            // When
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                ConfigSerializer.Write(config, writer);
            }

            // Then
            Assert.That(builder.ToString(), Contains.Substring("include-footer: false"));
        }

        [Test]
        public void Should_WriteSample_Keys_Without_Values()
        {
            // Given
            var builder = new StringBuilder();

            // When
            using (var writer = new StringWriter(builder))
            {
                ConfigSerializer.WriteSample(writer);
            }

            var text = builder.ToString();

            // Then
            Assert.That(text, Contains.Substring("#create:" + Environment.NewLine));
            Assert.That(text, Contains.Substring("#export:" + Environment.NewLine));
        }

        [Test]
        public void Should_WriteSample_Boolean_Values()
        {
            // Given
            var builder = new StringBuilder();

            // When
            using (var writer = new StringWriter(builder))
            {
                ConfigSerializer.WriteSample(writer);
            }

            var text = builder.ToString();

            // Then
            Assert.That(text, Contains.Substring("#  include-footer: true"));
        }

        [Test]
        public void Should_WriteSample_String_Values()
        {
            // Given
            var builder = new StringBuilder();

            // When
            using (var writer = new StringWriter(builder))
            {
                ConfigSerializer.WriteSample(writer);
            }

            var text = builder.ToString();

            // Then
            Assert.That(text, Contains.Substring("#  footer-heading: Where to get it"));
        }

        [Test]
        public void Should_WriteSample_Multiline_String_Values()
        {
            // Given
            var builder = new StringBuilder();

            // When
            using (var writer = new StringWriter(builder))
            {
                ConfigSerializer.WriteSample(writer);
            }

            var text = builder.ToString();

            // Then
            var expectedText = string.Format("#  footer-content: >-{0}#    You can download this release from{0}#{0}#    [chocolatey](https://chocolatey.org/packages/chocolateyGUI/{{milestone}})", Environment.NewLine);
            Assert.That(text, Contains.Substring(expectedText));
        }
    }
}