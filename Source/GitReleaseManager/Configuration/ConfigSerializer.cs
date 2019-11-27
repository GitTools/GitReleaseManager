//-----------------------------------------------------------------------
// <copyright file="ConfigSerializer.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using System;
    using System.IO;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public static class ConfigSerializer
    {
        public static Config Read(TextReader reader)
        {
            var deserializerBuilder = new DeserializerBuilder().WithNamingConvention(new HyphenatedNamingConvention());
            var deserializer = deserializerBuilder.Build();
            var deserialize = deserializer.Deserialize<Config>(reader);

            if (deserialize == null)
            {
                return new Config();
            }

            return deserialize;
        }

        public static void Write(Config config, TextWriter writer)
        {
            var serializerBuilder = new SerializerBuilder()
                .WithNamingConvention(new HyphenatedNamingConvention())
                .EmitDefaults(); // Can be removed when YamlDotNet is updated to 8.0.0+
            var serializer = serializerBuilder.Build();
            serializer.Serialize(writer, config);
        }

        // TODO: Need to expand this to include all options
        public static void WriteSample(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteLine(@"# create:");
            writer.WriteLine(@"#   include-footer: true | false");
            writer.WriteLine(@"#   footer-heading: Where to get it");
            writer.WriteLine(@"#   footer-content: You can download this release from [chocolatey](https://chocolatey.org/packages/chocolateyGUI/{milestone}");
            writer.WriteLine(@"#   footer-includes-milestone: true | false");
            writer.WriteLine(@"#   milestone-replace-text: '{milestone}'");
            writer.WriteLine(@"# export:");
            writer.WriteLine(@"#   include-created-date-in-title: true | false");
            writer.WriteLine(@"#   created-date-string-format: MMMM dd, yyyy");
            writer.WriteLine(@"#   perform-regex-removal: true | false");
            writer.WriteLine(@"#   regex-text: '### Where to get it(\r\n)*You can .*\)'");
            writer.WriteLine(@"#   multiline-regex: true | false");
            writer.WriteLine(@"# issue-labels-include:");
            writer.WriteLine(@"# - Bug");
            writer.WriteLine(@"# - Duplicate");
            writer.WriteLine(@"# - Enhancement");
            writer.WriteLine(@"# - Feature");
            writer.WriteLine(@"# - Help Wanted");
            writer.WriteLine(@"# - Improvement");
            writer.WriteLine(@"# - Invalid");
            writer.WriteLine(@"# - Question");
            writer.WriteLine(@"# - WontFix");
            writer.WriteLine(@"# issue-labels-exclude:");
            writer.WriteLine(@"# - Internal Refactoring");
        }
    }
}