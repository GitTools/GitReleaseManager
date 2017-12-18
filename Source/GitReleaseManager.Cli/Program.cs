//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CommandLine;
    using GitReleaseManager.Cli.Options;
    using GitReleaseManager.Core;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Helpers;
    using Octokit;
    using FileMode = System.IO.FileMode;

    public static class Program
    {
        private static StringBuilder log = new StringBuilder();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not required")]
        private static int Main(string[] args)
        {
            var fileSystem = new FileSystem();

            return Parser.Default.ParseArguments<CreateSubOptions, AddAssetSubOptions, CloseSubOptions, PublishSubOptions, ExportSubOptions, InitSubOptions, ShowConfigSubOptions>(args)
    .MapResult(
      (CreateSubOptions opts) => CreateReleaseAsync(opts, fileSystem).Result,
      (AddAssetSubOptions opts) => AddAssetsAsync(opts).Result,
      (CloseSubOptions opts) => CloseMilestoneAsync(opts).Result,
      (PublishSubOptions opts) => PublishReleaseAsync(opts).Result,
      (ExportSubOptions opts) => ExportReleasesAsync(opts, fileSystem).Result,
      (InitSubOptions opts) => CreateSampleConfigFile(opts, fileSystem),
      (ShowConfigSubOptions opts) => ShowConfig(opts, fileSystem),
            errs => 1);
        }

        private static async Task<int> CreateReleaseAsync(CreateSubOptions subOptions, IFileSystem fileSystem)
        {
            try
            {
                ConfigureLogging(subOptions.LogFilePath);

                var github = subOptions.CreateGitHubClient();
                var configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, fileSystem);

                Release release;
                if (!string.IsNullOrEmpty(subOptions.Milestone))
                {
                    release = await CreateReleaseFromMilestone(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease, configuration);
                }
                else
                {
                    release = await CreateReleaseFromInputFile(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Name, subOptions.InputFilePath, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease);
                }

                Console.WriteLine(release.HtmlUrl);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<int> AddAssetsAsync(AddAssetSubOptions subOptions)
        {
            try
            {
                ConfigureLogging(subOptions.LogFilePath);

                var github = subOptions.CreateGitHubClient();

                await AddAssets(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName, subOptions.AssetPaths);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<int> CloseMilestoneAsync(CloseSubOptions subOptions)
        {
            try
            {
                ConfigureLogging(subOptions.LogFilePath);

                var github = subOptions.CreateGitHubClient();

                await CloseMilestone(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<int> PublishReleaseAsync(PublishSubOptions subOptions)
        {
            try
            {
                ConfigureLogging(subOptions.LogFilePath);

                var github = subOptions.CreateGitHubClient();

                await PublishRelease(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<int> ExportReleasesAsync(ExportSubOptions subOptions, IFileSystem fileSystem)
        {
            try
            {
                ConfigureLogging(subOptions.LogFilePath);

                var github = subOptions.CreateGitHubClient();
                var configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, fileSystem);

                var releasesMarkdown = await ExportReleases(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName, configuration);

                using (var sw = new StreamWriter(File.Open(subOptions.FileOutputPath, FileMode.OpenOrCreate)))
                {
                    sw.Write(releasesMarkdown);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static int CreateSampleConfigFile(InitSubOptions subOptions, IFileSystem fileSystem)
        {
            ConfigureLogging(subOptions.LogFilePath);

            ConfigurationProvider.WriteSample(subOptions.TargetDirectory ?? Environment.CurrentDirectory, fileSystem);
            return 0;
        }

        private static int ShowConfig(ShowConfigSubOptions subOptions, IFileSystem fileSystem)
        {
            ConfigureLogging(subOptions.LogFilePath);

            Console.WriteLine(ConfigurationProvider.GetEffectiveConfigAsString(subOptions.TargetDirectory ?? Environment.CurrentDirectory, fileSystem));
            return 0;
        }

        private static async Task<Release> CreateReleaseFromMilestone(GitHubClient github, string owner, string repository, string milestone, string targetCommitish, IList<string> assets, bool prerelease, Config configuration)
        {
            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(github, owner, repository), owner, repository, milestone, configuration);

            var result = await releaseNotesBuilder.BuildReleaseNotes();

            var releaseUpdate = CreateNewRelease(milestone, result, prerelease, targetCommitish);

            var release = await github.Release.Create(owner, repository, releaseUpdate);

            await AddAssets(github, assets, release);

            return release;
        }

        private static async Task<Release> CreateReleaseFromInputFile(GitHubClient github, string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new ArgumentException("Unable to locate input file.");
            }

            var inputFileContents = File.ReadAllText(inputFilePath);

            var releaseUpdate = CreateNewRelease(name, inputFileContents, prerelease, targetCommitish);

            var release = await github.Release.Create(owner, repository, releaseUpdate);

            await AddAssets(github, assets, release);

            return release;
        }

        private static async Task AddAssets(GitHubClient github, string owner, string repository, string tagName, IList<string> assets)
        {
            var releases = await github.Release.GetAll(owner, repository);

            var release = releases.FirstOrDefault(r => r.TagName == tagName);

            if (release == null)
            {
                Logger.WriteError("Unable to find Release with specified tagName");
                return;
            }

            await AddAssets(github, assets, release);
        }

        private static async Task<string> ExportReleases(GitHubClient github, string owner, string repository, string tagName, Config configuration)
        {
            var releaseNotesExporter = new ReleaseNotesExporter(new DefaultGitHubClient(github, owner, repository), configuration);

            var result = await releaseNotesExporter.ExportReleaseNotes(tagName);

            return result;
        }

        private static async Task CloseMilestone(GitHubClient github, string owner, string repository, string milestoneTitle)
        {
            var milestoneClient = github.Issue.Milestone;
            var openMilestones = await milestoneClient.GetAllForRepository(owner, repository, new MilestoneRequest { State = ItemState.Open });
            var milestone = openMilestones.FirstOrDefault(m => m.Title == milestoneTitle);

            if (milestone == null)
            {
                return;
            }

            await milestoneClient.Update(owner, repository, milestone.Number, new MilestoneUpdate { State = ItemState.Closed });
        }

        private static async Task PublishRelease(GitHubClient github, string owner, string repository, string tagName)
        {
            var releases = await github.Release.GetAll(owner, repository);
            var release = releases.FirstOrDefault(r => r.TagName == tagName);

            if (release == null)
            {
                return;
            }

            var releaseUpdate = new ReleaseUpdate { TagName = tagName, Draft = false };

            await github.Release.Edit(owner, repository, release.Id, releaseUpdate);
        }

        private static async Task AddAssets(GitHubClient github, IList<string> assets, Release release)
        {
            if (assets != null)
            {
                foreach (var asset in assets)
                {
                    if (!File.Exists(asset))
                    {
                        continue;
                    }

                    var upload = new ReleaseAssetUpload
                    {
                        FileName = Path.GetFileName(asset),
                        ContentType = "application/octet-stream",
                        RawData = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    };

                    await github.Release.UploadAsset(release, upload);

                    // Make sure to tidy up the stream that was created above
                    upload.RawData.Dispose();
                }
            }
        }

        private static NewRelease CreateNewRelease(string name, string body, bool prerelease, string targetCommitish)
        {
            var newRelease = new NewRelease(name)
            {
                Draft = true,
                Body = body,
                Name = name,
                Prerelease = prerelease
            };

            if (!string.IsNullOrEmpty(targetCommitish))
            {
                newRelease.TargetCommitish = targetCommitish;
            }

            return newRelease;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not required.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is required here.")]
        private static void ConfigureLogging(string logFilePath)
        {
            var writeActions = new List<Action<string>>
            {
                s => log.AppendLine(s)
            };

            if (!string.IsNullOrEmpty(logFilePath))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
                    if (File.Exists(logFilePath))
                    {
                        using (File.CreateText(logFilePath))
                        {
                        }
                    }

                    writeActions.Add(x => WriteLogEntry(logFilePath, x));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to configure logging: " + ex.Message);
                }
            }
            else
            {
                // if nothing else is specified, write to console
                writeActions.Add(Console.WriteLine);
            }

            Logger.WriteInfo = s => writeActions.ForEach(a => a(s));
            Logger.WriteWarning = s => writeActions.ForEach(a => a(s));
            Logger.WriteError = s => writeActions.ForEach(a => a(s));
        }

        private static void WriteLogEntry(string logFilePath, string s)
        {
            var contents = string.Format(CultureInfo.InvariantCulture, "{0}\t\t{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), s);
            File.AppendAllText(logFilePath, contents);
        }
    }
}