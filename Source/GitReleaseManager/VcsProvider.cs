namespace GitReleaseManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Model;
    using Serilog;

    public abstract class VcsProvider : IVcsProvider
    {
        public VcsProvider(Config configuration, ILogger logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        protected Config Configuration { get; }

        protected ILogger Logger { get; }

        public abstract Task AddAssets(string owner, string repository, string tagName, IList<string> assets);

        public abstract Task CloseMilestone(string owner, string repository, string milestoneTitle);

        public abstract Task<Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease);

        public abstract Task<Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease);

        public abstract Task DiscardRelease(string owner, string repository, string name);

        public abstract Task<string> ExportReleases(string owner, string repository, string tagName);

        public abstract string GetCommitsLink(string user, string repository, Milestone milestone, Milestone previousMilestone);

        public abstract Task<List<Issue>> GetIssuesAsync(Milestone targetMilestone);

        public abstract Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone, string user, string repository);

        public abstract Task<ReadOnlyCollection<Milestone>> GetReadOnlyMilestonesAsync(string user, string repository);

        public abstract Task<List<Release>> GetReleasesAsync(string user, string repository);

        public abstract Task<Release> GetSpecificRelease(string tagName, string user, string repository);

        public abstract Task OpenMilestone(string owner, string repository, string milestoneTitle);

        public abstract Task PublishRelease(string owner, string repository, string tagName);

        public virtual async Task CreateLabels(string owner, string repository)
        {
            if (Configuration.Labels.Any())
            {
                Logger.Verbose("Removing existing labels");
                await DeleteExistingLabelsAsync(owner, repository).ConfigureAwait(false);
                Logger.Verbose("Creating new standard labels");
                var newLabelTasks = new List<Task>();
                foreach (var label in Configuration.Labels)
                {
                    newLabelTasks.Add(CreateLabelAsync(owner, repository, label));
                }

                await Task.WhenAll(newLabelTasks).ConfigureAwait(false);
            }
            else
            {
                Logger.Warning("No labels defined");
            }
        }

        protected abstract Task DeleteExistingLabelsAsync(string owner, string repository);

        protected abstract Task CreateLabelAsync(string owner, string repository, LabelConfig label);

        protected abstract Task<List<string>> GetCommentsForIssueAsync(string owner, string repository, long index);

        protected virtual async Task<bool> DoesAnyCommentIncludeStringAsync(string owner, string repository, long index, string comment)
        {
            Logger.Verbose("Finding issue comment created by GitReleaseManager for issue #{IssueNumber}", index);
            var issueComments = await GetCommentsForIssueAsync(owner, repository, index).ConfigureAwait(false);

            return issueComments.Any(c => c.Contains(comment));
        }

    }
}
