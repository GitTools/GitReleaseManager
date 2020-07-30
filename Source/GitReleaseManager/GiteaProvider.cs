using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using GitReleaseManager.Core.Configuration;
using GitReleaseManager.Core.Model;
using Serilog;
using Gitea.Client;
using Gitea.Api;
using Gitea.Model;
using System.Diagnostics.SymbolStore;

namespace GitReleaseManager.Core
{
    public class GiteaProvider : IVcsProvider
    {
        private readonly ILogger _logger = Log.ForContext<GiteaProvider>();
        private readonly string _token;
        private readonly Config _configuration;
        private readonly ApiClient _apiClient;

        public GiteaProvider(Config configuration, string token, string basePath)
        {
            _configuration = configuration;
            _token = token;
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

        public Task CloseMilestone(string owner, string repository, string milestoneTitle)
        {
            throw new NotImplementedException();
        }

        public async Task CreateLabels(string owner, string repository)
        {
            var issueApi = new IssueApi(Gitea.Client.Configuration.Default);
            var currentLabelList = await issueApi.IssueListLabelsAsync(owner, repository).ConfigureAwait(false);
            var deleteLabelTasks = new List<Task>();
            foreach (var label in currentLabelList)
            {
                 deleteLabelTasks.Add(issueApi.IssueDeleteLabelAsync(owner, repository, label.Id));
            }

            await Task.WhenAll(deleteLabelTasks.ToArray()).ConfigureAwait(false);
            var createLabelTasks = new List<Task>();
            foreach (var label in _configuration.Labels)
            {
                createLabelTasks.Add(issueApi.IssueCreateLabelAsync(owner, repository, new CreateLabelOption(label.Color, label.Description, label.Name)));
            }

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
    }
}