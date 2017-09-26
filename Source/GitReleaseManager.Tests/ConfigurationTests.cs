//-----------------------------------------------------------------------
// <copyright file="ConfigurationTests.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
{
    using System.IO;
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
        public void Many_Labels_For_Issue_Disabled_By_Default()
        {
            // Given
            var text = Resources.Default_Configuration_Yaml;

            // When
            var config = ConfigSerializer.Read(new StringReader(text));

            // Then
            Assert.AreEqual(false, config.IssueLabelsMany);
        }

        [Test]
        public void Should_Read_Many_Labels_Issue()
        {
            // Given
            var text = Resources.Many_Labels_For_Issue_Configuration_Yaml;

            // When
            var config = ConfigSerializer.Read(new StringReader(text));

            // Then
            Assert.AreEqual(true, config.IssueLabelsMany);
        }
    }
}
