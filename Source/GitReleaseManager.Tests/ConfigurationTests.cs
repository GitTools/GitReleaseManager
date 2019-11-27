//-----------------------------------------------------------------------
// <copyright file="ConfigurationTests.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
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

            // When
            var config = ConfigSerializer.Read(new StringReader(text));

            // Then
            Assert.AreEqual(2, config.LabelAliases.Count);
            Assert.AreEqual("Bug", config.LabelAliases[0].Name);
            Assert.AreEqual("Foo", config.LabelAliases[0].Header);
            Assert.AreEqual("Bar", config.LabelAliases[0].Plural);
            Assert.AreEqual("Improvement", config.LabelAliases[1].Name);
            Assert.AreEqual("Baz", config.LabelAliases[1].Header);
            Assert.AreEqual("Qux", config.LabelAliases[1].Plural);
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
    }
}
