//-----------------------------------------------------------------------
// <copyright file="ConfigSerializer.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Configuration
{
    using System;
    using System.IO;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public static class ConfigSerializer
    {
        public static Config Read(TextReader reader)
        {
            var deserializer = new Deserializer(null, new HyphenatedNamingConvention());
            var deserialize = deserializer.Deserialize<Config>(reader);
            
            if (deserialize == null)
            {
                return new Config();
            }

            return deserialize;
        }

        public static void Write(Config config, TextWriter writer)
        {
            var serializer = new Serializer(SerializationOptions.None, new HyphenatedNamingConvention());
            serializer.Serialize(writer, config);
        }

        // TODO: Need to expand this to include all options
        public static void WriteSample(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            writer.WriteLine(@"# export-regex: ### Where to get it(\r\n)*You can .*\)");
        }
    }
}