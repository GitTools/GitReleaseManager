//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="gep13">
//     Copyright (c) 2015 - Present Gary Ewan Park
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CommandLine;
    using GitHubReleaseManager.Cli.Options;
    using GitHubReleaseManager.Core;
    using GitHubReleaseManager.Core.Configuration;
    using GitHubReleaseManager.Core.Helpers;
    using Octokit;
    using FileMode = System.IO.FileMode;

    public static class Program
    {
        private static StringBuilder log = new StringBuilder();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not required")]
        private static int Main(string[] args)
        {
            var options = new MainOptions();

            var result = 1;

            if (!Parser.Default.ParseArgumentsStrict(
                args,
                options,
                (verb, subOptions) =>
                {
                    result = 1;

                    var baseSubOptions = subOptions as BaseSubOptions;
                    if (baseSubOptions != null)
                    {
                        if (string.IsNullOrEmpty(baseSubOptions.TargetDirectory))
                        {
                            baseSubOptions.TargetDirectory = Environment.CurrentDirectory;
                        }

                        ConfigureLogging(baseSubOptions.LogFilePath);
                    }

                    var fileSystem = new FileSystem();

                    if (verb == "create")
                    {
                        var createSubOptions = baseSubOptions as CreateSubOptions;
                        if (createSubOptions != null)
                        {
                            result = CreateReleaseAsync(createSubOptions, fileSystem).Result;
                        }
                    }

                    if (verb == "addasset")
                    {
                        var addAssetSubOptions = baseSubOptions as AddAssetSubOptions;
                        if (addAssetSubOptions != null)
                        {
                            result = AddAssetsAsync(addAssetSubOptions).Result;
                        }
                    }

                    if (verb == "close")
                    {
                        var closeSubOptions = baseSubOptions as CloseSubOptions;
                        if (closeSubOptions != null)
                        {
                            result = CloseMilestoneAsync(closeSubOptions).Result;
                        }
                    }

                    if (verb == "publish")
                    {
                        var publishSubOptions = baseSubOptions as PublishSubOptions;
                        if (publishSubOptions != null)
                        {
                            result = PublishReleaseAsync(publishSubOptions).Result;
                        }
                    }

                    if (verb == "export")
                    {
                        var exportSubOptions = baseSubOptions as ExportSubOptions;
                        if (exportSubOptions != null)
                        {
                            result = ExportReleasesAsync(exportSubOptions, fileSystem).Result;
                        }
                    }

                    if (verb == "init")
                    {
                        var initSubOptions = baseSubOptions as InitSubOptions;
                        if (initSubOptions != null)
                        {
                            ConfigurationProvider.WriteSample(initSubOptions.TargetDirectory, fileSystem);
                            result = 0;
                        }
                    }

                    if (verb == "showconfig")
                    {
                        var showConfigSubOptions = baseSubOptions as ShowConfigSubOptions;
                        if (showConfigSubOptions != null)
                        {
                            Console.WriteLine(ConfigurationProvider.GetEffectiveConfigAsString(showConfigSubOptions.TargetDirectory, fileSystem));
                            result = 0;
                        }
                    }
                }))
            {
                return 1;
            }

            return result;
        }

        private static async Task<int> CreateReleaseAsync(CreateSubOptions subOptions, IFileSystem fileSystem)
        {
            try
            {
                var github = subOptions.CreateGitHubClient();
                var configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory, fileSystem);

                if (!string.IsNullOrEmpty(subOptions.Milestone))
                {
                    await CreateReleaseFromMilestone(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease, configuration);
                }
                else
                {
                    await CreateReleaseFromInputFile(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Name, subOptions.InputFilePath, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease);
                }

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
                var github = subOptions.CreateGitHubClient();
                var configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory, fileSystem);

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

        private static async Task CreateReleaseFromMilestone(GitHubClient github, string owner, string repository, string milestone, string targetCommitish, IList<string> assets, bool prerelease, Config configuration)
        {
            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(github, owner, repository), owner, repository, milestone, configuration);

            var result = await releaseNotesBuilder.BuildReleaseNotes();

            var releaseUpdate = CreateReleaseUpdate(milestone, result, prerelease, targetCommitish);

            var release = await github.Release.Create(owner, repository, releaseUpdate);

            await AddAssets(github, assets, release);
        }

        private static async Task CreateReleaseFromInputFile(GitHubClient github, string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new ArgumentException("Unable to locate input file.");
            }

            var inputFileContents = File.ReadAllText(inputFilePath);

            var releaseUpdate = CreateReleaseUpdate(name, inputFileContents, prerelease, targetCommitish);

            var release = await github.Release.Create(owner, repository, releaseUpdate);

            await AddAssets(github, assets, release);
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
            var openMilestones = await milestoneClient.GetForRepository(owner, repository, new MilestoneRequest { State = ItemState.Open });
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

            var releaseUpdate = new ReleaseUpdate(tagName)
            {
                Draft = false
            };

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
                                         RawData = File.Open(asset, FileMode.Open)
                                     };

                    await github.Release.UploadAsset(release, upload);
                }
            }
        }

        private static ReleaseUpdate CreateReleaseUpdate(string name, string body, bool prerelease, string targetCommitish)
        {
            var releaseUpdate = new ReleaseUpdate(name)
            {
                Draft = true,
                Body = body,
                Name = name,
                Prerelease = prerelease
            };

            if (!string.IsNullOrEmpty(targetCommitish))
            {
                releaseUpdate.TargetCommitish = targetCommitish;
            }

            return releaseUpdate;
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