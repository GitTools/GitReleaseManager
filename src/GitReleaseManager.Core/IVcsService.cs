using System.Collections.Generic;
using System.Threading.Tasks;
using GitReleaseManager.Core.Model;

namespace GitReleaseManager.Core
{
    public interface IVcsService
    {
        Task<Release> CreateEmptyReleaseAsync(string owner, string repository, string name, string targetCommitish, bool prerelease);

        Task<Release> CreateReleaseFromMilestoneAsync(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease, string templateFilePath);

        Task<Release> CreateReleaseFromInputFileAsync(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease);

        Task DiscardReleaseAsync(string owner, string repository, string tagName);

        Task AddAssetsAsync(string owner, string repository, string tagName, IList<string> assets);

        Task<string> ExportReleasesAsync(string owner, string repository, string tagName, bool skipPrereleases);

        Task CloseMilestoneAsync(string owner, string repository, string milestoneTitle);

        Task OpenMilestoneAsync(string owner, string repository, string milestoneTitle);

        Task PublishReleaseAsync(string owner, string repository, string tagName);

        Task CreateLabelsAsync(string owner, string repository);
    }
}