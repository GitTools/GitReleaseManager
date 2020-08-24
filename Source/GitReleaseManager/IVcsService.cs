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
        Task<Release> CreateReleaseFromMilestoneAsync(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease);

        Task<Release> CreateReleaseFromInputFileAsync(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease);

        Task DiscardReleaseAsync(string owner, string repository, string tagName);

        Task AddAssetsAsync(string owner, string repository, string tagName, IList<string> assets);

        Task<string> ExportReleasesAsync(string owner, string repository, string tagName);

        Task CloseMilestoneAsync(string owner, string repository, string milestoneTitle);

        Task OpenMilestoneAsync(string owner, string repository, string milestoneTitle);

        Task PublishReleaseAsync(string owner, string repository, string tagName);

        Task CreateLabelsAsync(string owner, string repository);
    }
}