namespace GitReleaseManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Gitea.Api;
    using Gitea.Client;
    using Gitea.Model;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Extensions;
    using Serilog;

    public class GiteaProvider : IVcsProvider
    {
        private readonly ILogger _logger = Log.ForContext<GiteaProvider>();
        private readonly Config _configuration;

        public GiteaProvider(Config configuration, string token, string basePath)
        {
            _configuration = configuration;
            /*
             * This will result in a HTTP header like this: "Authorization: token xxx"
             * As per https://docs.gitea.io/en-us/api-usage/
             */
            Gitea.Client.Configuration.Default.AddApiKeyPrefix("Authorization", "token");
            Gitea.Client.Configuration.Default.AddApiKey("Authorization", token);
            Gitea.Client.Configuration.Default.BasePath = basePath;
        }

        public Task AddAssets(string owner, string repository, string tagName, IList<string> assets)
        {
            throw new NotImplementedException();
        }

        public async Task CloseMilestone(string owner, string repository, string milestoneTitle)
        {
            _logger.Verbose("Finding open milestone with title '{Title}' on '{Owner}/{Repository}'", milestoneTitle, owner, repository);
            var api = new IssueApi();

            // find the requested milestone
            var milestones = await api.IssueGetMilestonesListAsync(owner, repository).ConfigureAwait(false);
            var milestone = milestones.FirstOrDefault(x => x.Title.Equals(milestoneTitle, StringComparison.InvariantCulture));
            if (milestone == null)
            {
                // if no match has been found, return
                _logger.Debug("No existing open milestone with title '{Title}' was found", milestoneTitle);
                return;
            }

            // close the milestone
            _logger.Verbose("Closing milestone '{Title}' on '{Owner}/{Repository}'", milestoneTitle, owner, repository);
            await api.IssueEditMilestoneAsync(owner, repository, milestone.Id, new EditMilestoneOption() { State = "closed" }).ConfigureAwait(false);

            if (_configuration.Close.IssueComments)
            {
                // if configured accordingly, add a "issue has been fixed in this milestone" comment to all the issues in the milestone
                const string detectionComment = "<!-- GitReleaseManager release comment -->";

                // prepare the body for the comment
                var issueComment = detectionComment + "\n" + _configuration.Close.IssueCommentFormat.ReplaceTemplate(new { owner, repository, Milestone = milestone.Title });

                // get all the closed issues
                var issues = await api.IssueListIssuesAsync(owner, repository, "closed", null, null, null, milestone.Title).ConfigureAwait(false);

                foreach (var issue in issues)
                {
                    try
                    {
                        if (!await CommentsIncludeString(api, owner, repository, issue.Number, detectionComment).ConfigureAwait(false))
                        {
                            // if no generated comment exists yet, create one
                            _logger.Information("Adding release comment for issue #{IssueNumber}", issue.Number);
                            await api.IssueCreateCommentAsync(owner, repository, issue.Number, new CreateIssueCommentOption(issueComment)).ConfigureAwait(false);
                        }
                        else
                        {
                            _logger.Information("Issue #{IssueNumber} already contains release comment, skipping...", issue.Number);
                        }
                    }
                    catch (ApiException ex)
                    {
                        _logger.Error(ex, "Unable to add comment to issue #{IssueNumber}.", issue.Number);
                        break;
                    }
                }
            }
        }

        public async Task CreateLabels(string owner, string repository)
        {
            _logger.Information("Deleting all existing labels");
            var issueApi = new IssueApi();

            // get a list containing all the labels
            var currentLabelList = await issueApi.IssueListLabelsAsync(owner, repository).ConfigureAwait(false);
            var deleteLabelTasks = new List<Task>();
            foreach (var label in currentLabelList)
            {
                // delete each label
                 deleteLabelTasks.Add(issueApi.IssueDeleteLabelAsync(owner, repository, label.Id));
            }

            // wait for all the deletion tasks to complete
            await Task.WhenAll(deleteLabelTasks.ToArray()).ConfigureAwait(false);
            _logger.Information("Creating new labels");

            // create new labels based on the configuration
            var createLabelTasks = new List<Task>();
            foreach (var label in _configuration.Labels)
            {
                createLabelTasks.Add(issueApi.IssueCreateLabelAsync(owner, repository, new CreateLabelOption(label.Color, label.Description, label.Name)));
            }

            // wait for all tasks before returning
            await Task.WhenAll(createLabelTasks).ConfigureAwait(false);
        }

        public Task<Model.Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new NotImplementedException();
        }

        public Task<Model.Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new NotImplementedException();
        }

        public Task DiscardRelease(string owner, string repository, string name)
        {
            throw new NotImplementedException();
        }

        public Task<string> ExportReleases(string owner, string repository, string tagName)
        {
            throw new NotImplementedException();
        }

        public string GetCommitsLink(string user, string repository, Model.Milestone milestone, Model.Milestone previousMilestone)
        {
            throw new NotImplementedException();
        }

        public Task<List<Model.Issue>> GetIssuesAsync(Model.Milestone targetMilestone)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetNumberOfCommitsBetween(Model.Milestone previousMilestone, Model.Milestone currentMilestone, string user, string repository)
        {
            throw new NotImplementedException();
        }

        public Task<ReadOnlyCollection<Model.Milestone>> GetReadOnlyMilestonesAsync(string user, string repository)
        {
            throw new NotImplementedException();
        }

        public Task<List<Model.Release>> GetReleasesAsync(string user, string repository)
        {
            throw new NotImplementedException();
        }

        public Task<Model.Release> GetSpecificRelease(string tagName, string user, string repository)
        {
            throw new NotImplementedException();
        }

        public Task OpenMilestone(string owner, string repository, string milestoneTitle)
        {
            throw new NotImplementedException();
        }

        public Task PublishRelease(string owner, string repository, string tagName)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> CommentsIncludeString(IssueApi api, string owner, string repository, long index, string comment)
        {
            _logger.Verbose("Finding issue comment created by GitReleaseManager for issue #{IssueNumber}", index);
            var issueComments = await api.IssueGetCommentsAsync(owner, repository, index).ConfigureAwait(false);

            return issueComments.Any(c => c.Body.Contains(comment));
        }
    }
}