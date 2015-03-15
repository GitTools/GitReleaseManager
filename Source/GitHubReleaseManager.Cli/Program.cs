//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="gep13">
//     Copyright (c) 2015 - Present gep13
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
    using GitHubReleaseManager.Configuration;
    using GitHubReleaseManager.Helpers;
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
                            if (string.IsNullOrEmpty(baseSubOptions.TargetPath))
                            {
                                baseSubOptions.TargetPath = Environment.CurrentDirectory;
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
                                result = AddAssetAsync(addAssetSubOptions).Result;
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
                                result = CloseAndPublishReleaseAsync(publishSubOptions).Result;
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
                                ConfigurationProvider.WriteSample(initSubOptions.TargetPath, fileSystem);
                                result = 0;
                            }
                        }

                        if (verb == "showconfig")
                        {
                            var showConfigSubOptions = baseSubOptions as ShowConfigSubOptions;
                            if (showConfigSubOptions != null)
                            {
                                Console.WriteLine(ConfigurationProvider.GetEffectiveConfigAsString(showConfigSubOptions.TargetPath, fileSystem));
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
                var configuration = ConfigurationProvider.Provide(subOptions.TargetPath, fileSystem);

                await CreateRelease(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone, subOptions.TargetCommitish, subOptions.AssetPath, configuration);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<int> AddAssetAsync(AddAssetSubOptions subOptions)
        {
            try
            {
                var github = subOptions.CreateGitHubClient();

                await AddAsset(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone, subOptions.AssetPath);

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

        private static async Task<int> CloseAndPublishReleaseAsync(PublishSubOptions subOptions)
        {
            try
            {
                var github = subOptions.CreateGitHubClient();

                await CloseMilestone(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone);

                await PublishRelease(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone);

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
                var configuration = ConfigurationProvider.Provide(subOptions.TargetPath, fileSystem);

                var releasesMarkdown = await ExportReleases(github, subOptions.RepositoryOwner, subOptions.RepositoryName, configuration);

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

        private static async Task CreateRelease(GitHubClient github, string owner, string repository, string milestone, string targetCommitish, string asset, Config configuration)
        {
            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(github, owner, repository), owner, repository, milestone, configuration);

            var result = await releaseNotesBuilder.BuildReleaseNotes();

            var releaseUpdate = new ReleaseUpdate(milestone)
            {
                Draft = true,
                Body = result,
                Name = milestone
            };
            if (!string.IsNullOrEmpty(targetCommitish))
            {
                releaseUpdate.TargetCommitish = targetCommitish;
            }

            var release = await github.Release.Create(owner, repository, releaseUpdate);

            if (File.Exists(asset))
            {
                var upload = new ReleaseAssetUpload { FileName = Path.GetFileName(asset), ContentType = "application/octet-stream", RawData = File.Open(asset, FileMode.Open) };

                await github.Release.UploadAsset(release, upload);
            }
        }

        private static async Task AddAsset(GitHubClient github, string owner, string repository, string milestone, string assetPath)
        {
            var releases = await github.Release.GetAll(owner, repository);

            var release = releases.FirstOrDefault(r => r.TagName == milestone);

            if (release == null)
            {
                Logger.WriteError("Unable to find Release with specified milestone");
                return;
            }

            if (File.Exists(assetPath))
            {
                var upload = new ReleaseAssetUpload { FileName = Path.GetFileName(assetPath), ContentType = "application/octet-stream", RawData = File.Open(assetPath, FileMode.Open) };
                await github.Release.UploadAsset(release, upload);
            }
        }

        private static async Task<string> ExportReleases(GitHubClient github, string owner, string repository, Config configuration)
        {
            var releaseNotesExporter = new ReleaseNotesExporter(new DefaultGitHubClient(github, owner, repository), configuration);

            var result = await releaseNotesExporter.ExportReleaseNotes();

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

        private static async Task PublishRelease(GitHubClient github, string owner, string repository, string milestone)
        {
            var releases = await github.Release.GetAll(owner, repository);
            var release = releases.FirstOrDefault(r => r.Name == milestone);

            if (release == null)
            {
                return;
            }

            var releaseUpdate = new ReleaseUpdate(milestone)
            {
                Draft = false
            };

            await github.Release.Edit(owner, repository, release.Id, releaseUpdate);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is required here")]
        private static void ConfigureLogging(string logFilePath)
        {
            var writeActions = new List<Action<string>>
            {
                s => log.AppendLine(s)
            };

            if (logFilePath == "console")
            {
                writeActions.Add(Console.WriteLine);
            }
            else if (!string.IsNullOrEmpty(logFilePath))
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