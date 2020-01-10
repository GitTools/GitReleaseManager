//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Cli
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using AutoMapper;
    using CommandLine;
    using GitReleaseManager.Cli.Logging;
    using GitReleaseManager.Cli.Options;
    using GitReleaseManager.Core;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using Serilog;

    public static class Program
    {
        private static FileSystem _fileSystem;
        private static Config _configuration;
        private static IMapper _mapper;
        private static IVcsProvider _vcsProvider;

        private static async Task<int> Main(string[] args)
        {
            // Just add the TLS 1.2 protocol to the Service Point manager until
            // we've upgraded to latest Octokit.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            _fileSystem = new FileSystem();

            _mapper = AutoMapperConfiguration.Configure();

            try
            {
                return await Parser.Default.ParseArguments<CreateSubOptions, DiscardSubOptions, AddAssetSubOptions, CloseSubOptions, PublishSubOptions, ExportSubOptions, InitSubOptions, ShowConfigSubOptions, LabelSubOptions>(args)
                    .WithParsed<BaseSubOptions>(LogConfiguration.ConfigureLogging)
                    .WithParsed<BaseSubOptions>(CreateFiglet)
                    .WithParsed<BaseSubOptions>(LogOptions)
                    .MapResult(
                    (CreateSubOptions opts) => CreateReleaseAsync(opts),
                    (DiscardSubOptions opts) => DiscardReleaseAsync(opts),
                    (AddAssetSubOptions opts) => AddAssetsAsync(opts),
                    (CloseSubOptions opts) => CloseMilestoneAsync(opts),
                    (OpenSubOptions opts) => OpenMilestoneAsync(opts),
                    (PublishSubOptions opts) => PublishReleaseAsync(opts),
                    (ExportSubOptions opts) => ExportReleasesAsync(opts),
                    (InitSubOptions opts) => CreateSampleConfigFileAsync(opts),
                    (ShowConfigSubOptions opts) => ShowConfigAsync(opts),
                    (LabelSubOptions opts) => CreateLabelsAsync(opts),
                    errs => Task.FromResult(1)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "{Message}", ex.Message);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void CreateFiglet(BaseSubOptions options)
        {
            if (options.NoLogo)
            {
                return;
            }

            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            if (version.IndexOf('+') > 0)
            {
                version = version.Substring(0, version.IndexOf('+'));
            }
            // The following ugly formats is to prevent incorrect indentation
            // detected by editorconfig formatters.
            var shortFormat = "\n   ____ ____  __  __\n"
                + "  / ___|  _ \\|  \\/  |\n"
                + " | |  _| |_) | |\\/| |\n"
                + " | |_| |  _ <| |  | |\n"
                + "  \\____|_| \\_\\_|  |_|\n"
                + "{0,21}\n";
            var longFormat = "\n   ____ _ _   ____      _                     __  __\n"
                + "  / ___(_) |_|  _ \\ ___| | ___  __ _ ___  ___|  \\/  | __ _ _ __   __ _  __ _  ___ _ __\n"
                + " | |  _| | __| |_) / _ \\ |/ _ \\/ _` / __|/ _ \\ |\\/| |/ _` | '_ \\ / _` |/ _` |/ _ \\ '__|\n"
                + " | |_| | | |_|  _ <  __/ |  __/ (_| \\__ \\  __/ |  | | (_| | | | | (_| | (_| |  __/ |\n"
                + "  \\____|_|\\__|_| \\_\\___|_|\\___|\\__,_|___/\\___|_|  |_|\\__,_|_| |_|\\__,_|\\__, |\\___|_|\n"
                + "                                                                       |___/\n"
                + "{0,87}\n";
            if (Console.WindowWidth > 87)
            {
                Log.Information(longFormat, version);
            }
            else
            {
                Log.Information(shortFormat, version);
            }
        }

        private static async Task<int> CreateReleaseAsync(CreateSubOptions subOptions)
        {
            _vcsProvider = GetVcsProvider(subOptions);

            Core.Model.Release release;
            if (!string.IsNullOrEmpty(subOptions.Milestone))
            {
                var releaseName = subOptions.Name;
                if (string.IsNullOrWhiteSpace(releaseName))
                {
                    releaseName = subOptions.Milestone;
                }

                release = await _vcsProvider.CreateReleaseFromMilestone(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone, releaseName, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease).ConfigureAwait(false);
            }
            else
            {
                release = await _vcsProvider.CreateReleaseFromInputFile(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Name, subOptions.InputFilePath, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease).ConfigureAwait(false);
            }

            Log.Information("Drafted release is available at:\n{HtmlUrl}", release.HtmlUrl);
            Log.Verbose("Body:\n{Body}", release.Body);
            return 0;
        }

        private static async Task<int> DiscardReleaseAsync(DiscardSubOptions subOptions)
        {
            _vcsProvider = GetVcsProvider(subOptions);

            await _vcsProvider.DiscardRelease(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone);

            return 0;
        }

        private static async Task<int> AddAssetsAsync(AddAssetSubOptions subOptions)
        {
            _vcsProvider = GetVcsProvider(subOptions);

            await _vcsProvider.AddAssets(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName, subOptions.AssetPaths).ConfigureAwait(false);

            return 0;
        }

        private static async Task<int> CloseMilestoneAsync(CloseSubOptions subOptions)
        {
            _vcsProvider = GetVcsProvider(subOptions);

            await _vcsProvider.CloseMilestone(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone).ConfigureAwait(false);

            return 0;
        }

        private static async Task<int> OpenMilestoneAsync(OpenSubOptions subOptions)
        {
            _vcsProvider = GetVcsProvider(subOptions);

            await _vcsProvider.OpenMilestone(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone).ConfigureAwait(false);

            return 0;
        }

        private static async Task<int> PublishReleaseAsync(PublishSubOptions subOptions)
        {
            _vcsProvider = GetVcsProvider(subOptions);

            await _vcsProvider.PublishRelease(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName).ConfigureAwait(false);
            return 0;
        }

        private static async Task<int> ExportReleasesAsync(ExportSubOptions subOptions)
        {
            _vcsProvider = GetVcsProvider(subOptions);

            var releasesMarkdown = await _vcsProvider.ExportReleases(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName).ConfigureAwait(false);

            using (var sw = new StreamWriter(File.Open(subOptions.FileOutputPath, FileMode.OpenOrCreate)))
            {
                sw.Write(releasesMarkdown);
            }

            return 0;
        }

        private static Task<int> CreateSampleConfigFileAsync(InitSubOptions subOptions)
        {
            ConfigurationProvider.WriteSample(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);
            return Task.FromResult(0);
        }

        private static Task<int> ShowConfigAsync(ShowConfigSubOptions subOptions)
        {
            Log.Information(ConfigurationProvider.GetEffectiveConfigAsString(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem));
            return Task.FromResult(0);
        }

        private static async Task<int> CreateLabelsAsync(LabelSubOptions subOptions)
        {
            _vcsProvider = GetVcsProvider(subOptions);

            await _vcsProvider.CreateLabels(subOptions.RepositoryOwner, subOptions.RepositoryName).ConfigureAwait(false);
            return 0;
        }

        private static IVcsProvider GetVcsProvider(BaseVcsOptions subOptions)
        {
            _configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);
            return new GitHubProvider(_mapper, _configuration, subOptions.UserName, subOptions.Password, subOptions.Token);
        }

        private static void LogOptions(BaseSubOptions options)
            => Log.Verbose("{@Options}", options);
    }
}