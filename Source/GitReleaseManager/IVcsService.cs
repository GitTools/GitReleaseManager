//-----------------------------------------------------------------------
// <copyright file="IVcsService.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Model;

    public interface IVcsService
    {
        Task<Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease);

        Task<Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease);

        Task DiscardRelease(string owner, string repository, string tagName);

        Task AddAssets(string owner, string repository, string tagName, IList<string> assets);

        Task<string> ExportReleases(string owner, string repository, string tagName);

        Task CloseMilestone(string owner, string repository, string milestoneTitle);

        Task OpenMilestone(string owner, string repository, string milestoneTitle);

        Task PublishRelease(string owner, string repository, string tagName);

        Task CreateLabels(string owner, string repository);
    }
}