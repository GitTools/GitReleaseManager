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
    using System.Net;
    using System.Security.Cryptography;
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
        private static StringBuilder _log = new StringBuilder();
        private static FileSystem _fileSystem;
        private static Config _configuration;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not required")]
        private static int Main(string[] args)
        {
            // Just add the TLS 1.2 protocol to the Service Point manager until
            // we've upgraded to latest Octokit.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            _fileSystem = new FileSystem();

            return Parser.Default.ParseArguments<CreateSubOptions, AddAssetSubOptions, CloseSubOptions, PublishSubOptions, ExportSubOptions, InitSubOptions, ShowConfigSubOptions, LabelSubOptions>(args)
                .MapResult(
                  (CreateSubOptions opts) => CreateReleaseAsync(opts).Result,
                  (AddAssetSubOptions opts) => AddAssetsAsync(opts).Result,
                  (CloseSubOptions opts) => CloseMilestoneAsync(opts).Result,
                  (PublishSubOptions opts) => PublishReleaseAsync(opts).Result,
                  (ExportSubOptions opts) => ExportReleasesAsync(opts).Result,
                  (InitSubOptions opts) => CreateSampleConfigFile(opts),
                  (ShowConfigSubOptions opts) => ShowConfig(opts),
                  (LabelSubOptions opts) => CreateLabelsAsync(opts).Result,
                  errs => 1);
        }

        private static async Task<int> CreateReleaseAsync(CreateSubOptions subOptions)
        {
            try
            {
                ConfigureLogging(subOptions.LogFilePath);

                var github = subOptions.CreateGitHubClient();
                _configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);

                Release release;
                if (!string.IsNullOrEmpty(subOptions.Milestone))
                {
                    var releaseName = subOptions.Name;
                    if (string.IsNullOrWhiteSpace(releaseName))
                    {
                        releaseName = subOptions.Milestone;
                    }

                    release = await CreateReleaseFromMilestone(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.Milestone, releaseName, subOptions.TargetCommitish, subOptions.AssetPaths, subOptions.Prerelease);
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
                _configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);

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
                _configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);

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
                _configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);

                await PublishRelease(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<int> ExportReleasesAsync(ExportSubOptions subOptions)
        {
            try
            {
                ConfigureLogging(subOptions.LogFilePath);

                var github = subOptions.CreateGitHubClient();
                _configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);

                var releasesMarkdown = await ExportReleases(github, subOptions.RepositoryOwner, subOptions.RepositoryName, subOptions.TagName);

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

        private static int CreateSampleConfigFile(InitSubOptions subOptions)
        {
            ConfigureLogging(subOptions.LogFilePath);

            ConfigurationProvider.WriteSample(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);
            return 0;
        }

        private static int ShowConfig(ShowConfigSubOptions subOptions)
        {
            ConfigureLogging(subOptions.LogFilePath);

            Console.WriteLine(ConfigurationProvider.GetEffectiveConfigAsString(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem));
            return 0;
        }

        private static async Task<int> CreateLabelsAsync(LabelSubOptions subOptions)
        {
            try
            {
                ConfigureLogging(subOptions.LogFilePath);

                var newLabels = new List<NewLabel>();
                newLabels.Add(new NewLabel("Breaking change", "b60205"));
                newLabels.Add(new NewLabel("Bug", "ee0701"));
                newLabels.Add(new NewLabel("Build", "009800"));
                newLabels.Add(new NewLabel("Documentation", "d4c5f9"));
                newLabels.Add(new NewLabel("Feature", "84b6eb"));
                newLabels.Add(new NewLabel("Improvement", "207de5"));
                newLabels.Add(new NewLabel("Question", "cc317c"));
                newLabels.Add(new NewLabel("good first issue", "7057ff"));
                newLabels.Add(new NewLabel("help wanted", "33aa3f"));

                var github = subOptions.CreateGitHubClient();
                _configuration = ConfigurationProvider.Provide(subOptions.TargetDirectory ?? Environment.CurrentDirectory, _fileSystem);

                var labels = await github.Issue.Labels.GetAllForRepository(subOptions.RepositoryOwner, subOptions.RepositoryName);

                foreach (var label in labels)
                {
                    await github.Issue.Labels.Delete(subOptions.RepositoryOwner, subOptions.RepositoryName, label.Name);
                }

                foreach (var label in newLabels)
                {
                    await github.Issue.Labels.Create(subOptions.RepositoryOwner, subOptions.RepositoryName, label);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<Release> CreateReleaseFromMilestone(GitHubClient github, string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease)
        {
            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(github, owner, repository), owner, repository, milestone, _configuration);

            var result = await releaseNotesBuilder.BuildReleaseNotes();

            var releaseUpdate = CreateNewRelease(releaseName, milestone, result, prerelease, targetCommitish);

            var release = await github.Repository.Release.Create(owner, repository, releaseUpdate);

            await AddAssets(github, owner, repository, assets, release);

            return release;
        }

        private static async Task<Release> CreateReleaseFromInputFile(GitHubClient github, string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new ArgumentException("Unable to locate input file.");
            }

            var inputFileContents = File.ReadAllText(inputFilePath);

            var releaseUpdate = CreateNewRelease(name, name, inputFileContents, prerelease, targetCommitish);

            var release = await github.Repository.Release.Create(owner, repository, releaseUpdate);

            await AddAssets(github, owner, repository, assets, release);

            return release;
        }

        private static async Task AddAssets(GitHubClient github, string owner, string repository, string tagName, IList<string> assets)
        {
            var releases = await github.Repository.Release.GetAll(owner, repository);

            var release = releases.FirstOrDefault(r => r.TagName == tagName);

            if (release == null)
            {
                Logger.WriteError("Unable to find Release with specified tagName");
                return;
            }

            await AddAssets(github, owner, repository, assets, release);
        }

        private static async Task<string> ExportReleases(GitHubClient github, string owner, string repository, string tagName)
        {
            var releaseNotesExporter = new ReleaseNotesExporter(new DefaultGitHubClient(github, owner, repository), _configuration);

            var result = await releaseNotesExporter.ExportReleaseNotes(tagName);

            return result;
        }

        private static async Task CloseMilestone(GitHubClient github, string owner, string repository, string milestoneTitle)
        {
            var milestoneClient = github.Issue.Milestone;
            var openMilestones = await milestoneClient.GetAllForRepository(owner, repository, new MilestoneRequest { State = ItemStateFilter.Open });
            var milestone = openMilestones.FirstOrDefault(m => m.Title == milestoneTitle);

            if (milestone == null)
            {
                return;
            }

            await milestoneClient.Update(owner, repository, milestone.Number, new MilestoneUpdate { State = ItemState.Closed });
        }

        private static async Task PublishRelease(GitHubClient github, string owner, string repository, string tagName)
        {
            var releases = await github.Repository.Release.GetAll(owner, repository);
            var release = releases.FirstOrDefault(r => r.TagName == tagName);

            if (release == null)
            {
                return;
            }

            var releaseUpdate = new ReleaseUpdate { TagName = tagName, Draft = false };

            await github.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate);
        }

        private static async Task AddAssets(GitHubClient github, string owner, string repository, IList<string> assets, Release release)
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

                    await github.Repository.Release.UploadAsset(release, upload);

                    // Make sure to tidy up the stream that was created above
                    upload.RawData.Dispose();
                }

                await AddAssetsSha256(github, owner, repository, assets, release);
            }
        }

        private static async Task AddAssetsSha256(GitHubClient github, string owner, string repository, IList<string> assets, Release release)
        {
            if (assets != null && assets.Any() && _configuration.Create.IncludeShaSection)
            {
                var stringBuilder = new StringBuilder(release.Body);

                if (!release.Body.Contains(_configuration.Create.ShaSectionHeading))
                {
                    stringBuilder.AppendLine(string.Format("### {0}", _configuration.Create.ShaSectionHeading));
                }

                foreach (var asset in assets)
                {
                    var file = new FileInfo(asset);

                    if (!file.Exists)
                    {
                        continue;
                    }

                    stringBuilder.AppendFormat(_configuration.Create.ShaSectionLineFormat, file.Name, ComputeSha256Hash(asset));
                    stringBuilder.AppendLine();
                }

                stringBuilder.AppendLine();

                var releaseUpdate = release.ToUpdate();
                releaseUpdate.Body = stringBuilder.ToString();
                await github.Repository.Release.Edit(owner, repository, release.Id, releaseUpdate);
            }
        }

        private static NewRelease CreateNewRelease(string name, string tagName, string body, bool prerelease, string targetCommitish)
        {
            var newRelease = new NewRelease(tagName)
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
                s => _log.AppendLine(s)
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

        private static string ComputeSha256Hash(string asset)
        {
            // Create a SHA256
            using (var sha256Hash = SHA256.Create())
            {
                using (var fileStream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // ComputeHash - returns byte array
                    var bytes = sha256Hash.ComputeHash(fileStream);

                    // Convert byte array to a string
                    var builder = new StringBuilder();

                    foreach (var t in bytes)
                    {
                        builder.Append(t.ToString("x2"));
                    }

                    return builder.ToString();
                }
            }
        }
    }
}