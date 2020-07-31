namespace GitReleaseManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Gitea.Api;
    using Gitea.Client;
    using Gitea.Model;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Extensions;
    using Serilog;

    public class GiteaProvider : BaseVcsProvider
    {
        private readonly IssueApi _api;

        public GiteaProvider(IMapper mapper, Config configuration, string token, string basePath)
            : base(mapper, configuration, Log.ForContext<GiteaProvider>())
        {
            /*
             * This will result in a HTTP header like this: "Authorization: token xxx"
             * As per https://docs.gitea.io/en-us/api-usage/
             */
            Gitea.Client.Configuration.Default.AddApiKeyPrefix("Authorization", "token");
            Gitea.Client.Configuration.Default.AddApiKey("Authorization", token);
            Gitea.Client.Configuration.Default.BasePath = basePath;
            _api = new IssueApi();
        }

        public override Task AddAssets(string owner, string repository, string tagName, IList<string> assets)
        {
            throw new NotImplementedException();
        }

        public override Task<Model.Release> CreateReleaseFromInputFile(string owner, string repository, string name, string inputFilePath, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new NotImplementedException();
        }

        public override Task<Model.Release> CreateReleaseFromMilestone(string owner, string repository, string milestone, string releaseName, string targetCommitish, IList<string> assets, bool prerelease)
        {
            throw new NotImplementedException();
        }

        public override Task DiscardRelease(string owner, string repository, string name)
        {
            throw new NotImplementedException();
        }

        public override Task<string> ExportReleases(string owner, string repository, string tagName)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<Model.Issue>> GetClosedIssuesForMilestoneAsync(string owner, string repository, Model.Milestone targetMilestone)
        {
            var issues = await _api.IssueListIssuesAsync(owner, repository, "closed", null, null, null, targetMilestone.Title).ConfigureAwait(false);
            return Mapper.Map<List<Issue>, List<Model.Issue>>(issues);
        }

        public override string GetCommitsLink(string user, string repository, Model.Milestone milestone, Model.Milestone previousMilestone)
        {
            throw new NotImplementedException();
        }

        public override Task<int> GetNumberOfCommitsBetween(Model.Milestone previousMilestone, Model.Milestone currentMilestone, string user, string repository)
        {
            throw new NotImplementedException();
        }

        public override Task<ReadOnlyCollection<Model.Milestone>> GetReadOnlyMilestonesAsync(string user, string repository)
        {
            throw new NotImplementedException();
        }

        public override Task<List<Model.Release>> GetReleasesAsync(string user, string repository)
        {
            throw new NotImplementedException();
        }

        public override Task<Model.Release> GetSpecificRelease(string tagName, string user, string repository)
        {
            throw new NotImplementedException();
        }

        public override Task OpenMilestone(string owner, string repository, string milestoneTitle)
        {
            throw new NotImplementedException();
        }

        public override Task PublishRelease(string owner, string repository, string tagName)
        {
            throw new NotImplementedException();
        }

        protected override async Task CloseMilestoneAsync(string owner, string repository, Model.Milestone milestone)
        {
            // todo: milestone.Number should be number
            await _api.IssueEditMilestoneAsync(owner, repository, Convert.ToInt64(milestone.Number), new EditMilestoneOption() { State = "closed" }).ConfigureAwait(false);
        }

        protected override async Task CreateCommentAsync(string owner, string repository, Model.Issue issue, string comment)
        {
            await _api.IssueCreateCommentAsync(owner, repository, Convert.ToInt64(issue.Number), new CreateIssueCommentOption(comment)).ConfigureAwait(false);
        }

        protected override async Task CreateLabelAsync(string owner, string repository, LabelConfig label)
        {
            await _api.IssueCreateLabelAsync(owner, repository, new CreateLabelOption(label.Color, label.Description, label.Name)).ConfigureAwait(false);
        }

        protected override async Task DeleteExistingLabelsAsync(string owner, string repository)
        {
            Logger.Information("Deleting all existing labels");

            // get a list containing all the labels
            var currentLabelList = await _api.IssueListLabelsAsync(owner, repository).ConfigureAwait(false);
            var deleteLabelTasks = new List<Task>();
            foreach (var label in currentLabelList)
            {
                // delete each label
                deleteLabelTasks.Add(_api.IssueDeleteLabelAsync(owner, repository, label.Id));
            }

            // wait for all the deletion tasks to complete
            await Task.WhenAll(deleteLabelTasks.ToArray()).ConfigureAwait(false);
        }

        protected override async Task<List<string>> GetCommentsForIssueAsync(string owner, string repository, long index)
        {
            var comments = await _api.IssueGetCommentsAsync(owner, repository, index).ConfigureAwait(false);
            var result = new List<string>();
            foreach (var comment in comments)
            {
                result.Add(comment.Body);
            }

            return result;
        }

        protected override async Task<Model.Milestone> GetMilestoneAsync(string owner, string repository, string milestoneTitle)
        {
            var milestones = await _api.IssueGetMilestonesListAsync(owner, repository).ConfigureAwait(false);
            var milestone = milestones.FirstOrDefault(x => x.Title.Equals(milestoneTitle, StringComparison.InvariantCulture));
            return Mapper.Map<Model.Milestone>(milestone);
        }
    }
}