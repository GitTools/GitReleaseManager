//-----------------------------------------------------------------------
// <copyright file="ConfigSerializer.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using System;
    using System.IO;
    using System.Reflection;
    using GitReleaseManager.Core.Attributes;
    using GitReleaseManager.Core.Configuration.CommentSerialization;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public static class ConfigSerializer
    {
        public static Config Read(TextReader reader)
        {
            var deserializerBuilder = new DeserializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance);
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
                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                .WithNamingConvention(HyphenatedNamingConvention.Instance);
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

            var config = new Config();
            config.LabelAliases.Add(new LabelAlias
            {
                Header = "Documentation",
                Name = "Documentation",
                Plural = "Documentation",
            });
            var configType = config.GetType();

            writer.Write("#");
            writer.NewLine = Environment.NewLine + "#";
            SetConfigurationSamples(config, configType, writer);

            Write(config, writer);
        }

        private static void SetConfigurationSamples(object config, Type configType, TextWriter writer)
        {
            foreach (var property in configType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var sampleAttribute = property.GetCustomAttribute<SampleAttribute>();
                var propertyType = property.PropertyType;

                if (propertyType.IsClass && propertyType != typeof(string))
                {
                    var subConfig = property.GetValue(config);
                    SetConfigurationSamples(subConfig, propertyType, writer);
                }
                else if (sampleAttribute != null && propertyType == sampleAttribute.Value.GetType())
                {
                    // We need to replace '\n' newlines in samples when running on Windows to keep
                    // Line endings consistent
                    var value = Environment.OSVersion.Platform == PlatformID.Win32NT && propertyType == typeof(string)
                        ? sampleAttribute.Value.ToString().Replace("\n", Environment.NewLine)
                        : sampleAttribute.Value;
                    property.SetValue(config, value);
                }
            }
        }
    }
}