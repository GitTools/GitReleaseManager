namespace GitReleaseManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Model;
    using GitReleaseManager.Core.Extensions;
    using Serilog;
    
    public abstract class BaseVcsProvider : IVcsProvider
    {
        public BaseVcsProvider(IMapper mapper, Config configuration, ILogger logger)
        {
            Mapper = mapper;
            Configuration = configuration;
            Logger = logger;
        }
        protected IMapper Mapper { get; }

        protected Config Configuration { get; }

        protected ILogger Logger { get; }

        public abstract Task AddAssets(string owner, string repository, string tagName, IList<string> assets);


        public abstract Task<Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease);

        public abstract Task<Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease);

        public abstract Task DiscardRelease(string owner, string repository, string name);

        public abstract Task<string> ExportReleases(string owner, string repository, string tagName);

        public abstract string GetCommitsLink(string user, string repository, Milestone milestone, Milestone previousMilestone);

        public abstract Task<List<Issue>> GetClosedIssuesForMilestoneAsync(string owner, string repository, Milestone targetMilestone);

        public abstract Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone, string user, string repository);

        public abstract Task<ReadOnlyCollection<Milestone>> GetReadOnlyMilestonesAsync(string user, string repository);

        public abstract Task<List<Release>> GetReleasesAsync(string user, string repository);

        public abstract Task<Release> GetSpecificRelease(string tagName, string user, string repository);

        public abstract Task OpenMilestone(string owner, string repository, string milestoneTitle);

        public abstract Task PublishRelease(string owner, string repository, string tagName);

        public virtual async Task CloseAndCommentMilestoneAsync(string owner, string repository, string milestoneTitle)
        {
            Logger.Verbose("Finding open milestone with title '{Title}' on '{Owner}/{Repository}'", milestoneTitle, owner, repository);
            var milestone = await GetMilestoneAsync(owner, repository, milestoneTitle).ConfigureAwait(false);

            if (milestone == null)
            {
                // if no match has been found, return
                Logger.Debug("No existing open milestone with title '{Title}' was found", milestoneTitle);
                return;
            }

            // close the milestone
            Logger.Verbose("Closing milestone '{Title}' on '{Owner}/{Repository}'", milestoneTitle, owner, repository);
            await CloseMilestoneAsync(owner, repository, milestone).ConfigureAwait(false);

            if (Configuration.Close.IssueComments)
            {
                // if configured accordingly, add a "issue has been fixed in this milestone" comment to all the issues in the milestone
                const string detectionComment = "<!-- GitReleaseManager release comment -->";

                // prepare the body for the comment
                var issueComment = detectionComment + "\n" + Configuration.Close.IssueCommentFormat.ReplaceTemplate(new { owner, repository, Milestone = milestone.Title });

                // get all the closed issues
                var issues = await GetClosedIssuesForMilestoneAsync(owner, repository, milestone).ConfigureAwait(false);

                foreach (var issue in issues)
                {
                    // todo: issue.Number should be long
                    if (!await DoesAnyCommentIncludeStringAsync(owner, repository, Convert.ToInt64(issue.Number), detectionComment).ConfigureAwait(false))
                    {
                        // if no generated comment exists yet, create one
                        Logger.Information("Adding release comment for issue #{IssueNumber}", issue.Number);
                        try
                        {
                            await CreateCommentAsync(owner, repository, issue, issueComment).ConfigureAwait(false); // _api.IssueCreateCommentAsync(owner, repository, issue.Number, new CreateIssueCommentOption(issueComment)).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Unable to add comment to issue #{IssueNumber}.", issue.Number);
                            break;
                        }
                    }
                    else
                    {
                        Logger.Information("Issue #{IssueNumber} already contains release comment, skipping...", issue.Number);
                    }
                }
            }
        }

        public virtual async Task CreateDefaultLabelsAsync(string owner, string repository)
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

        protected abstract Task CreateCommentAsync(string owner, string repository, Issue issue, string comment);

        protected abstract Task<List<string>> GetCommentsForIssueAsync(string owner, string repository, long index);

        protected abstract Task<Milestone> GetMilestoneAsync(string owner, string repository, string milestoneTitle);

        protected abstract Task CloseMilestoneAsync(string owner, string repository, Milestone milestone);


        protected virtual async Task<bool> DoesAnyCommentIncludeStringAsync(string owner, string repository, long index, string comment)
        {
            Logger.Verbose("Finding issue comment created by GitReleaseManager for issue #{IssueNumber}", index);
            var issueComments = await GetCommentsForIssueAsync(owner, repository, index).ConfigureAwait(false);
            return issueComments.Any(c => c.Contains(comment));
        }

    }
}
