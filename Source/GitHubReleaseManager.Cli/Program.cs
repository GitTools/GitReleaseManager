//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Cli
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CommandLine;
    using GitHubReleaseManager.Configuration;
    using GitHubReleaseManager.Helpers;
    using Octokit;
    using FileMode = System.IO.FileMode;

    public static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Not required")]
        private static int Main(string[] args)
        {
            var options = new Options();

            var result = 1;

            if (!Parser.Default.ParseArgumentsStrict(
                args,
                options,
                (verb, subOptions) =>
                    {
                        var fileSystem = new FileSystem();
                        var currentFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        var currentDirectory = Path.GetDirectoryName(currentFilePath);

                        if (verb == "create")
                        {
                            result = CreateReleaseAsync((CreateSubOptions)subOptions).Result;
                        }

                        if (verb == "close")
                        {
                            result = CloseMilestoneAsync((CloseSubOptions)subOptions).Result;
                        }

                        if (verb == "publish")
                        {
                            result = CloseAndPublishReleaseAsync((PublishSubOptions)subOptions).Result;
                        }

                        if (verb == "init")
                        {
                            ConfigurationProvider.WriteSample(currentDirectory, fileSystem);
                            result = 0;
                        }

                        if (verb == "showconfig")
                        {
                            Console.WriteLine(ConfigurationProvider.GetEffectiveConfigAsString(currentDirectory, fileSystem));
                            result = 0;
                        }
                    }))
            {
                return 1;
            }

            return result;
        }

        private static async Task<int> CreateReleaseAsync(CreateSubOptions options)
        {
            try
            {
                var github = options.CreateGitHubClient();

                await CreateRelease(github, options.RepositoryOwner, options.RepositoryName, options.Milestone, options.TargetCommitish, options.AssetPath);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<int> CloseMilestoneAsync(CloseSubOptions options)
        {
            try
            {
                var github = options.CreateGitHubClient();

                await CloseMilestone(github, options.RepositoryOwner, options.RepositoryName, options.Milestone);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task<int> CloseAndPublishReleaseAsync(PublishSubOptions options)
        {
            try
            {
                var github = options.CreateGitHubClient();

                await CloseMilestone(github, options.RepositoryOwner, options.RepositoryName, options.Milestone);

                await PublishRelease(github, options.RepositoryOwner, options.RepositoryName, options.Milestone);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return 1;
            }
        }

        private static async Task CreateRelease(GitHubClient github, string owner, string repository, string milestone, string targetCommitish, string asset)
        {
            var releaseNotesBuilder = new ReleaseNotesBuilder(new DefaultGitHubClient(github, owner, repository), owner, repository, milestone);

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
    }
}