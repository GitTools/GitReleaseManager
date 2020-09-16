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
    using Serilog;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public static class ConfigSerializer
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(ConfigSerializer));

        public static Config Read(TextReader reader)
        {
            _logger.Debug("Starting deserializing yaml configuration file...");
            var deserializerBuilder = new DeserializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance);
            var deserializer = deserializerBuilder.Build();
            var deserialize = deserializer.Deserialize<Config>(reader);

            if (deserialize is null)
            {
                _logger.Verbose("Deseriazing failed, returning default configuration!");
                deserialize = new Config();
            }
            else
            {
                _logger.Verbose("Successfully deserialized configuration!");
            }

            _logger.Debug("{@Config}", deserialize);

            return deserialize;
        }

        public static void Write(Config config, TextWriter writer)
        {
            _logger.Debug("Starting serializing yaml configuration...");
            var serializerBuilder = new SerializerBuilder()
                .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                .WithNamingConvention(HyphenatedNamingConvention.Instance);
            var serializer = serializerBuilder.Build();
            serializer.Serialize(writer, config);
            _logger.Verbose("Successfully serialized configuration!");
        }

        // TODO: Need to expand this to include all options
        public static void WriteSample(TextWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            _logger.Debug("Starting sample generation...");

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

            _logger.Verbose("Writing sample");
            Write(config, writer);
        }

        private static void SetConfigurationSamples(object config, Type configType, TextWriter writer)
        {
            foreach (var property in configType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var sampleAttribute = property.GetCustomAttribute<SampleAttribute>();
                var propertyType = property.PropertyType;

                if (propertyType.IsClass && propertyType != typeof(string) && propertyType != typeof(DirectoryInfo))
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
                    _logger.Debug("Found property: '{Name}' with value '{Value}'", property.Name, value);
                    property.SetValue(config, value);
                }
            }
        }
    }
}