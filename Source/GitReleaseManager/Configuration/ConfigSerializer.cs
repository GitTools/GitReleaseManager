//-----------------------------------------------------------------------
// <copyright file="ConfigSerializer.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.Configuration
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
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

            if (deserialize.IssueLabelsPrecedence.Count != 0)
            {
                ExpandLabelPrecedenceList(deserialize); 
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
            writer.WriteLine(@"# - Feature");
            writer.WriteLine(@"# - Improvement");
            writer.WriteLine(@"# issue-labels-exclude:");
            writer.WriteLine(@"# - Internal Refactoring");
        }

        private static void ExpandLabelPrecedenceList(Config config)
        {
            var occurrenceDictionary =
                config.IssueLabelsInclude.Select(lbl => new { Label = lbl, Include = true, Found = false })
                    .Concat(config.IssueLabelsExclude.Select(lbl => new { Label = lbl, Include = false, Found = false }))
                    .ToDictionary(x => x.Label, x => new { x.Include, x.Found });

            var origPrecedenceList = config.IssueLabelsPrecedence.ToList();
            config.IssueLabelsPrecedence.Clear();

            foreach (var label in origPrecedenceList)
            {
                switch (label)
                {
                    case "$include":
                        foreach (
                            var remaining in occurrenceDictionary.Where(kvp => !kvp.Value.Found && kvp.Value.Include).ToList())
                        {
                            occurrenceDictionary[remaining.Key] = new { remaining.Value.Include, Found = true };
                            config.IssueLabelsPrecedence.Add(remaining.Key);
                        }

                        break;
                    case "$exclude":
                        foreach (
                            var remaining in
                                occurrenceDictionary.Where(kvp => !kvp.Value.Found && !kvp.Value.Include).ToList())
                        {
                            occurrenceDictionary[remaining.Key] = new { remaining.Value.Include, Found = true };
                            config.IssueLabelsPrecedence.Add(remaining.Key);
                        }

                        break;
                    default:
                        if (!occurrenceDictionary.ContainsKey(label))
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Label {0} was not found in either the include or exclude list.",
                                    label));
                        }

                        var details = occurrenceDictionary[label];
                        if (details.Found)
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Label {0} is already listed in the precedence config.",
                                    label));
                        }

                        config.IssueLabelsPrecedence.Add(label);
                        occurrenceDictionary[label] = new { details.Include, Found = true };
                        break;
                }
            }
        }
    }
}