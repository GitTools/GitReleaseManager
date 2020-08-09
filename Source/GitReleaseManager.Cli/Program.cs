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
    using CommandLine;
    using GitReleaseManager.Cli.Logging;
    using GitReleaseManager.Cli.Options;
    using GitReleaseManager.Core;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using GitReleaseManager.Core.Provider;
    using GitReleaseManager.Core.ReleaseNotes;
    using Microsoft.Extensions.DependencyInjection;
    using Octokit;
    using Serilog;

    public static class Program
    {
        private static FileSystem _fileSystem;
        private static IVcsService _vcsService;
        private static IServiceProvider _serviceProvider;

        private static async Task<int> Main(string[] args)
        {
            // Just add the TLS 1.2 protocol to the Service Point manager until
            // we've upgraded to latest Octokit.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            _fileSystem = new FileSystem();

            try
            {
                return await Parser.Default.ParseArguments<CreateSubOptions, DiscardSubOptions, AddAssetSubOptions, CloseSubOptions, OpenSubOptions, PublishSubOptions, ExportSubOptions, InitSubOptions, ShowConfigSubOptions, LabelSubOptions>(args)
                    .WithParsed<BaseSubOptions>(LogConfiguration.ConfigureLogging)
                    .WithParsed<BaseSubOptions>(CreateFiglet)
                    .WithParsed<BaseSubOptions>(LogOptions)
                    .WithParsed<BaseVcsOptions>(ReportUsernamePasswordDeprecation)
                    .WithParsed<BaseVcsOptions>(RegisterServices)
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
            catch (AggregateException ex)
            {
                Log.Fatal("{Message}", ex.Message);
                foreach (var innerException in ex.InnerExceptions)
                {
                    Log.Fatal(innerException, "{Message}", innerException.Message);
                }

                return 1;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "{Message}", ex.Message);
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
                DisposeServices();
            }
        }

        private static void RegisterServices(BaseVcsOptions options)
        {
            var logger = Log.ForContext<VcsService>();
            var mapper = AutoMapperConfiguration.Configure();
            var configuration = ConfigurationProvider.Provide(options.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);

            var credentials = string.IsNullOrWhiteSpace(options.Token)
                ? new Credentials(options.UserName, options.Password)
                : new Credentials(options.Token);

            var gitHubClient = new GitHubClient(new ProductHeaderValue("GitReleaseManager")) { Credentials = credentials };

            var serviceCollection = new ServiceCollection()
                .AddSingleton(logger)
                .AddSingleton(mapper)
                .AddSingleton(configuration)
                .AddSingleton(configuration.Export)
                .AddSingleton<IReleaseNotesExporter, ReleaseNotesExporter>()
                .AddSingleton<IReleaseNotesBuilder, ReleaseNotesBuilder>()
                .AddSingleton<IGitHubClient>(gitHubClient)
                .AddSingleton<IVcsProvider, GitHubProvider>()
                .AddSingleton<IVcsService, VcsService>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void DisposeServices()
        {
            if (_serviceProvider is IDisposable serviceProvider)
            {
                serviceProvider.Dispose();
            }
        }

        private static void ReportUsernamePasswordDeprecation(BaseVcsOptions options)
        {
            if (!string.IsNullOrEmpty(options.UserName) || !string.IsNullOrEmpty(options.Password))
            {
                Log.Warning(BaseVcsOptions.OBSOLETE_MESSAGE);
            }
        }

        private static void CreateFiglet(BaseSubOptions options)
        {
            if (options.NoLogo)
            {
                return;
            }

            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            if (version.IndexOf('+') >= 0)
            {
                version = version.Substring(0, version.IndexOf('+'));
            }
            // The following ugly formats is to prevent incorrect indentation
            // detected by editorconfig formatters.
            const string shortFormat = "\n   ____ ____  __  __\n"
                + "  / ___|  _ \\|  \\/  |\n"
                + " | |  _| |_) | |\\/| |\n"
                + " | |_| |  _ <| |  | |\n"
                + "  \\____|_| \\_\\_|  |_|\n"
                + "{0,21}\n";
            const string longFormat = "\n   ____ _ _   ____      _                     __  __\n"
                + "  / ___(_) |_|  _ \\ ___| | ___  __ _ ___  ___|  \\/  | __ _ _ __   __ _  __ _  ___ _ __\n"
                + " | |  _| | __| |_) / _ \\ |/ _ \\/ _` / __|/ _ \\ |\\/| |/ _` | '_ \\ / _` |/ _` |/ _ \\ '__|\n"
                + " | |_| | | |_|  _ <  __/ |  __/ (_| \\__ \\  __/ |  | | (_| | | | | (_| | (_| |  __/ |\n"
                + "  \\____|_|\\__|_| \\_\\___|_|\\___|\\__,_|___/\\___|_|  |_|\\__,_|_| |_|\\__,_|\\__, |\\___|_|\n"
                + "                                                                       |___/\n"
                + "{0,87}\n";

            if (GetConsoleWidth() > 87)
            {
                Log.Information(longFormat, version);
            }
            else
            {
                Log.Information(shortFormat, version);
            }
        }

        private static int GetConsoleWidth()
        {
            try
            {
                return Console.WindowWidth;
            }
            catch
            {
                Log.Verbose("Unable to get the width of the console.");
            }

            try
            {
                return Console.BufferWidth;
            }
            catch
            {
                Log.Verbose("Unable to get the width of the buffer");
                return int.MaxValue;
            }
        }

        private static async Task<int> CreateReleaseAsync(CreateSubOptions subOptions)
        {
            Log.Information("Creating release...");
            _vcsService = GetVcsService();

            Core.Model.Release release;
            if (!string.IsNullOrEmpty(subOptions.Milestone))
            {
                Log.Verbose("Milestone {Milestone} was specified", subOptions.Milestone);
                var releaseName = subOptions.Name;
                if (string.IsNullOrWhiteSpace(releaseName))
                {
                    Log.Verbose("No Release Name was specified, using {Milestone}.", subOptions.Milestone);
                    releaseName = subOptions.Milestone;
                }

                release = await _vcsService.CreateReleaseFromMilestoneAsync(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone, releaseName, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease).ConfigureAwait(false);
            }
            else
            {
                Log.Verbose("No milestone was specified, switching to release creating from input file");
                release = await _vcsService.CreateReleaseFromInputFileAsync(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Name, subOptions.InputFilePath, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease).ConfigureAwait(false);
            }

            Log.Information("Drafted release is available at:\n{HtmlUrl}", release.HtmlUrl);
            Log.Verbose("Body:\n{Body}", release.Body);
            return 0;
        }

        private static async Task<int> DiscardReleaseAsync(DiscardSubOptions subOptions)
        {
            Log.Information("Discarding release {Milestone}", subOptions.Milestone);
            _vcsService = GetVcsService();

            await _vcsService.DiscardReleaseAsync(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone).ConfigureAwait(false);

            return 0;
        }

        private static async Task<int> AddAssetsAsync(AddAssetSubOptions subOptions)
        {
            Log.Information("Uploading assets");
            _vcsService = GetVcsService();

            await _vcsService.AddAssetsAsync(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName, subOptions.AssetPaths).ConfigureAwait(false);

            return 0;
        }

        private static async Task<int> CloseMilestoneAsync(CloseSubOptions subOptions)
        {
            Log.Information("Closing milestone {Milestone}", subOptions.Milestone);
            _vcsService = GetVcsService();

            await _vcsService.CloseMilestoneAsync(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone).ConfigureAwait(false);

            return 0;
        }

        private static async Task<int> OpenMilestoneAsync(OpenSubOptions subOptions)
        {
            Log.Information("Opening milestone {Milestone}", subOptions.Milestone);
            _vcsService = GetVcsService();

            await _vcsService.OpenMilestoneAsync(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone).ConfigureAwait(false);

            return 0;
        }

        private static async Task<int> PublishReleaseAsync(PublishSubOptions subOptions)
        {
            _vcsService = GetVcsService();

            await _vcsService.PublishReleaseAsync(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName).ConfigureAwait(false);
            return 0;
        }

        private static async Task<int> ExportReleasesAsync(ExportSubOptions subOptions)
        {
            Log.Information("Exporting release {TagName}", subOptions.TagName);
            _vcsService = GetVcsService();

            var releasesMarkdown = await _vcsService.ExportReleasesAsync(subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName).ConfigureAwait(false);

            using (var sw = new StreamWriter(File.Open(subOptions.FileOutputPath, System.IO.FileMode.OpenOrCreate)))
            {
                sw.Write(releasesMarkdown);
            }

            return 0;
        }

        private static Task<int> CreateSampleConfigFileAsync(InitSubOptions subOptions)
        {
            Log.Information("Creating sample configuration file");
            var directory = subOptions.TargetDirectory ?? Environment.CurrentDirectory;
            ConfigurationProvider.WriteSample(directory, _fileSystem);
            return Task.FromResult(0);
        }

        private static Task<int> ShowConfigAsync(ShowConfigSubOptions subOptions)
        {
            var configuration = ConfigurationProvider.GetEffectiveConfigAsString(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);

            Log.Information("{Configuration}", configuration);
            return Task.FromResult(0);
        }

        private static async Task<int> CreateLabelsAsync(LabelSubOptions subOptions)
        {
            Log.Information("Creating standard labels");
            _vcsService = GetVcsService();

            await _vcsService.CreateLabelsAsync(subOptions.RepositoryOwner, subOptions.RepositoryName).ConfigureAwait(false);
            return 0;
        }

        private static IVcsService GetVcsService()
        {
            return _serviceProvider.GetService<IVcsService>();
        }

        private static void LogOptions(BaseSubOptions options)
            => Log.Debug("{@Options}", options);
    }
}