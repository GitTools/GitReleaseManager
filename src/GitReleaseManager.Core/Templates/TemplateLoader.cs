// -----------------------------------------------------------------------
// <copyright file="TemplateLoader.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Templates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using AutoMapper.Configuration.Annotations;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using Scriban;
    using Scriban.Parsing;
    using Scriban.Runtime;

    public class TemplateLoader : ITemplateLoader
    {
        private static readonly string[] _templateExtensions = new[] { ".sbn", ".scriban" };
        private readonly Config _config;
        private readonly IFileSystem _fileSystem;
        private readonly TemplateKind _templateKind;

        public TemplateLoader(Config config, IFileSystem fileSystem, TemplateKind templateKind)
        {
            _config = config;
            _fileSystem = fileSystem;
            _templateKind = templateKind;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            IEnumerable<string> possiblePaths;
            if (Path.IsPathRooted(templateName))
            {
                if (!Path.HasExtension(templateName))
                {
                    possiblePaths = _templateExtensions.Select(e => templateName + e);
                }
                else
                {
                    possiblePaths = new[] { templateName };
                }
            }
            else
            {
                possiblePaths = GetFilePaths(context, _config, templateName, _templateKind);
            }

            foreach (var path in possiblePaths)
            {
                if (_fileSystem.Exists(path))
                {
                    return path;
                }
            }

            return GetResourcePath(context, templateName, _templateKind);
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            if (string.IsNullOrEmpty(templatePath))
            {
                return string.Empty;
            }

            if (templatePath.StartsWith(ReleaseTemplates.RESOURCE_PREFIX, StringComparison.InvariantCultureIgnoreCase))
            {
                return ReleaseTemplates.LoadTemplate(templatePath);
            }

            return _fileSystem.ReadAllText(templatePath);
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            var result = Load(context, callerSpan, templatePath);

            return await Task.FromResult(result).ConfigureAwait(false);
        }

        private static IEnumerable<string> GetFilePaths(TemplateContext context, Config config, string templateName, TemplateKind templateKind)
        {
            var extension = Path.GetExtension(templateName);
            var fileName = Path.GetFileNameWithoutExtension(templateName);

            // If additional template paths is added, this need to be updated
#pragma warning disable CA1308 // Normalize strings to uppercase
            string configName = templateKind.ToString().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
            var possibleExtensions = new List<string>(_templateExtensions);

            IEnumerable<string> testPaths;

            if (!string.IsNullOrEmpty(extension))
            {
                possibleExtensions.Clear();
                possibleExtensions.Add(extension);
            }
            else if (context != null)
            {
                extension = Path.GetExtension(context.CurrentSourceFile);
                if (!string.IsNullOrEmpty(extension))
                {
                    possibleExtensions.Insert(0, extension);
                }
            }

            if (context is null)
            {
                testPaths = GetTestPaths(config.TemplatesDirectoryInfo?.FullName ?? string.Empty, configName, fileName);
            }
            else if (context.CurrentSourceFile.StartsWith(ReleaseTemplates.RESOURCE_PREFIX, StringComparison.InvariantCultureIgnoreCase))
            {
                var currentSourceSub = context.CurrentSourceFile.Substring(ReleaseTemplates.RESOURCE_PREFIX.Length);

                // This should always be 3 items. config, template and previous resource name
                var splits = currentSourceSub.Split('/');
                Debug.Assert(splits.Length == 3, "Resource current source is not a 3-part length");
                testPaths = GetTestPaths(config.TemplatesDirectoryInfo?.FullName, splits[1], splits[0], fileName);
            }
            else
            {
                string directory = directory = Path.GetDirectoryName(context.CurrentSourceFile);

                testPaths = new[]
                {
                    Path.Combine(directory, fileName),
                    Path.Combine(directory, Path.GetDirectoryName(templateName), fileName),
                };
            }

            return testPaths.Distinct().SelectMany(t => possibleExtensions.Select(p => t + p));
        }

        private static IEnumerable<string> GetTestPaths(string baseDirectory, string configType, string templateName, string fileName = null)
        {
            if (fileName is null)
            {
                yield return Path.Combine(baseDirectory, templateName, configType, "index");
                yield return Path.Combine(baseDirectory, templateName, configType, templateName);
                yield return Path.Combine(baseDirectory, configType, templateName, "index");
                yield return Path.Combine(baseDirectory, configType, templateName, templateName);
            }
            else
            {
                yield return Path.Combine(baseDirectory, templateName, configType, fileName);
            }

            yield return Path.Combine(baseDirectory, configType, templateName, fileName ?? templateName);

            if (fileName is null)
            {
                yield return Path.Combine(baseDirectory, templateName, "index");
            }

            yield return Path.Combine(baseDirectory, templateName, fileName ?? templateName);

            if (fileName is null)
            {
                yield return Path.Combine(baseDirectory, configType, "index");
            }
            yield return Path.Combine(baseDirectory, configType, fileName ?? templateName);
            yield return Path.Combine(baseDirectory, fileName ?? templateName);
        }

        private static string GetResourcePath(TemplateContext context, string templateName, TemplateKind templateKind)
        {
            var fileName = Path.GetFileNameWithoutExtension(templateName);
#pragma warning disable CA1308 // Normalize strings to uppercase
            string configName = templateKind.ToString().ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            if (context is null)
            {
                return ReleaseTemplates.RESOURCE_PREFIX + string.Join("/", new[] { fileName, configName, "index" });
            }
            else
            {
                var directory = Path.GetDirectoryName(context.CurrentSourceFile);
                var directoryName = Path.GetFileNameWithoutExtension(directory);
                if (Enum.TryParse<TemplateKind>(directoryName, true, out _))
                {
                    directory = Path.GetDirectoryName(directory);
                    directoryName = Path.GetFileNameWithoutExtension(directory);
                }

                if (string.IsNullOrWhiteSpace(directoryName))
                {
                    throw new FileNotFoundException($"No file named '{fileName}' was found!");
                }

                return ReleaseTemplates.RESOURCE_PREFIX + string.Join(
                    "/",
                    new[] { directoryName, configName, fileName });
            }
        }
    }
}