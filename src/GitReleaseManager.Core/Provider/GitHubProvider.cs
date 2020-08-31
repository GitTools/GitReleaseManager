// -----------------------------------------------------------------------
// <copyright file="GitHubProvider.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Octokit;
    using ApiException = GitReleaseManager.Core.Exceptions.ApiException;
    using ForbiddenException = GitReleaseManager.Core.Exceptions.ForbiddenException;
    using Issue = GitReleaseManager.Core.Model.Issue;
    using IssueComment = GitReleaseManager.Core.Model.IssueComment;
    using ItemState = GitReleaseManager.Core.Model.ItemState;
    using ItemStateFilter = GitReleaseManager.Core.Model.ItemStateFilter;
    using Label = GitReleaseManager.Core.Model.Label;
    using Milestone = GitReleaseManager.Core.Model.Milestone;
    using NotFoundException = GitReleaseManager.Core.Exceptions.NotFoundException;
    using RateLimit = GitReleaseManager.Core.Model.RateLimit;
    using Release = GitReleaseManager.Core.Model.Release;
    using ReleaseAssetUpload = GitReleaseManager.Core.Model.ReleaseAssetUpload;

    public class GitHubProvider : IVcsProvider
    {
        private const int _pageSize = 100;
        private const string _notFoundMessgae = "NotFound";

        private readonly IGitHubClient _gitHubClient;
        private readonly IMapper _mapper;

        public GitHubProvider(IGitHubClient gitHubClient, IMapper mapper)
        {
            _gitHubClient = gitHubClient;
            _mapper = mapper;
        }

        public Task DeleteAssetAsync(string owner, string repository, int id)
        {
            return Execute(async () =>
            {
                await _gitHubClient.Repository.Release.DeleteAsset(owner, repository, id).ConfigureAwait(false);
            });
        }

        public Task UploadAssetAsync(Release release, ReleaseAssetUpload releaseAssetUpload)
        {
            return Execute(async () =>
            {
                var octokitRelease = _mapper.Map<Octokit.Release>(release);
                var octokitReleaseAssetUpload = _mapper.Map<Octokit.ReleaseAssetUpload>(releaseAssetUpload);

                await _gitHubClient.Repository.Release.UploadAsset(octokitRelease, octokitReleaseAssetUpload).ConfigureAwait(false);
            });
        }

        public Task<int> GetCommitsCount(string owner, string repository, string @base, string head)
        {
            return Execute(async () =>
            {
                try
                {
                    var result = await _gitHubClient.Repository.Commit.Compare(owner, repository, @base, head).ConfigureAwait(false);
                    return result.AheadBy;
                }
                catch (Octokit.NotFoundException)
                {
                    // If there is no tag yet the Compare will return a NotFoundException
                    // we can safely ignore
                    return 0;
                }
            });
        }

        public string GetCommitsUrl(string owner, string repository, string head, string @base = null)
        {
            Ensure.IsNotNullOrWhiteSpace(owner, nameof(owner));
            Ensure.IsNotNullOrWhiteSpace(repository, nameof(repository));
            Ensure.IsNotNullOrWhiteSpace(head, nameof(head));

            string url = string.IsNullOrWhiteSpace(@base)
                ? string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/commits/{2}", owner, repository, head)
                : string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/compare/{2}...{3}", owner, repository, @base, head);

            return url;
        }

        public Task CreateIssueCommentAsync(string owner, string repository, int issueNumber, string comment)
        {
            return Execute(async () =>
            {
                await _gitHubClient.Issue.Comment.Create(owner, repository, issueNumber, comment).ConfigureAwait(false);
            });
        }

        public Task<IEnumerable<Issue>> GetIssuesAsync(string owner, string repository, int milestoneNumber, ItemStateFilter itemStateFilter = ItemStateFilter.All)
        {
            return Execute(async () =>
            {
                var openIssueRequest = new RepositoryIssueRequest
                {
                    Milestone = milestoneNumber.ToString(CultureInfo.InvariantCulture),
                    State = (Octokit.ItemStateFilter)itemStateFilter,
                };

                var startPage = 1;
                var issues = new List<Octokit.Issue>();
                IReadOnlyList<Octokit.Issue> results;

                do
                {
                    var options = GetApiOptions(startPage);
                    results = await _gitHubClient.Issue.GetAllForRepository(owner, repository, openIssueRequest, options).ConfigureAwait(false);

                    issues.AddRange(results);
                    startPage++;
                }
                while (results.Count == _pageSize);

                return _mapper.Map<IEnumerable<Issue>>(issues);
            });
        }

        public Task<IEnumerable<IssueComment>> GetIssueCommentsAsync(string owner, string repository, int issueNumber)
        {
            return Execute(async () =>
            {
                var startPage = 1;
                var comments = new List<Octokit.IssueComment>();
                IReadOnlyList<Octokit.IssueComment> results;

                do
                {
                    var options = GetApiOptions(startPage);
                    results = await _gitHubClient.Issue.Comment.GetAllForIssue(owner, repository, issueNumber, options).ConfigureAwait(false);

                    comments.AddRange(results);
                    startPage++;
                }
                while (results.Count == _pageSize);

                return _mapper.Map<IEnumerable<IssueComment>>(comments);
            });
        }

        public Task CreateLabelAsync(string owner, string repository, Label label)
        {
            return Execute(async () =>
            {
                var newLabel = _mapper.Map<NewLabel>(label);

                await _gitHubClient.Issue.Labels.Create(owner, repository, newLabel).ConfigureAwait(false);
            });
        }

        public Task DeleteLabelAsync(string owner, string repository, string labelName)
        {
            return Execute(async () =>
            {
                await _gitHubClient.Issue.Labels.Delete(owner, repository, labelName).ConfigureAwait(false);
            });
        }

        public Task<IEnumerable<Label>> GetLabelsAsync(string owner, string repository)
        {
            return Execute(async () =>
            {
                var startPage = 1;
                var labels = new List<Octokit.Label>();
                IReadOnlyList<Octokit.Label> results;

                do
                {
                    var options = GetApiOptions(startPage);
                    results = await _gitHubClient.Issue.Labels.GetAllForRepository(owner, repository, options).ConfigureAwait(false);

                    labels.AddRange(results);
                    startPage++;
                }
                while (results.Count == _pageSize);

                return _mapper.Map<IEnumerable<Label>>(labels);
            });
        }

        public Task<Milestone> GetMilestoneAsync(string owner, string repository, string milestoneTitle, ItemStateFilter itemStateFilter = ItemStateFilter.All)
        {
            return Execute(async () =>
            {
                var milestones = await GetMilestonesAsync(owner, repository, itemStateFilter).ConfigureAwait(false);
                var milestone = milestones.FirstOrDefault(m => m.Title == milestoneTitle);

                if (milestone is null)
                {
                    throw new NotFoundException(_notFoundMessgae);
                }

                return milestone;
            });
        }

        public Task<IEnumerable<Milestone>> GetMilestonesAsync(string owner, string repository, ItemStateFilter itemStateFilter = ItemStateFilter.All)
        {
            return Execute(async () =>
            {
                var request = new MilestoneRequest { State = (Octokit.ItemStateFilter)itemStateFilter };

                var startPage = 1;
                var milestones = new List<Octokit.Milestone>();
                IReadOnlyList<Octokit.Milestone> results;

                do
                {
                    var options = GetApiOptions(startPage);
                    results = await _gitHubClient.Issue.Milestone.GetAllForRepository(owner, repository, request, options).ConfigureAwait(false);

                    milestones.AddRange(results);
                    startPage++;
                }
                while (results.Count == _pageSize);

                return _mapper.Map<IEnumerable<Milestone>>(milestones);
            });
        }

        public Task SetMilestoneStateAsync(string owner, string repository, int milestoneNumber, ItemState itemState)
        {
            return Execute(async () =>
            {
                var update = new MilestoneUpdate { State = (Octokit.ItemState)itemState };
                await _gitHubClient.Issue.Milestone.Update(owner, repository, milestoneNumber, update).ConfigureAwait(false);
            });
        }

        public Task<Release> CreateReleaseAsync(string owner, string repository, Release release)
        {
            return Execute(async () =>
            {
                var newRelease = _mapper.Map<NewRelease>(release);
                var octokitRelease = await _gitHubClient.Repository.Release.Create(owner, repository, newRelease).ConfigureAwait(false);

                return _mapper.Map<Release>(octokitRelease);
            });
        }

        public Task DeleteReleaseAsync(string owner, string repository, int id)
        {
            return Execute(async () =>
            {
                await _gitHubClient.Repository.Release.Delete(owner, repository, id).ConfigureAwait(false);
            });
        }

        public Task<Release> GetReleaseAsync(string owner, string repository, string tagName)
        {
            return Execute(async () =>
            {
                var release = await _gitHubClient.Repository.Release.Get(owner, repository, tagName).ConfigureAwait(false);

                return _mapper.Map<Release>(release);
            });
        }

        public Task<IEnumerable<Release>> GetReleasesAsync(string owner, string repository)
        {
            return Execute(async () =>
            {
                var startPage = 1;
                var releases = new List<Octokit.Release>();
                IReadOnlyList<Octokit.Release> results;

                do
                {
                    var options = GetApiOptions(startPage);
                    results = await _gitHubClient.Repository.Release.GetAll(owner, repository, options).ConfigureAwait(false);

                    releases.AddRange(results);
                    startPage++;
                }
                while (results.Count == _pageSize);

                releases = releases.OrderByDescending(r => r.CreatedAt).ToList();

                return _mapper.Map<IEnumerable<Release>>(releases);
            });
        }

        public Task PublishReleaseAsync(string owner, string repository, string tagName, int releaseId)
        {
            return Execute(async () =>
            {
                var update = new ReleaseUpdate
                {
                    Draft = false,
                    TagName = tagName,
                };

                await _gitHubClient.Repository.Release.Edit(owner, repository, releaseId, update).ConfigureAwait(false);
            });
        }

        public Task UpdateReleaseAsync(string owner, string repository, Release release)
        {
            return Execute(async () =>
            {
                var update = new ReleaseUpdate
                {
                    Body = release.Body,
                    Draft = release.Draft,
                    Name = release.Name,
                    Prerelease = release.Prerelease,
                    TagName = release.TagName,
                    TargetCommitish = release.TargetCommitish,
                };

                await _gitHubClient.Repository.Release.Edit(owner, repository, release.Id, update).ConfigureAwait(false);
            });
        }

        public RateLimit GetRateLimit()
        {
            try
            {
                var rateLimit = _gitHubClient.GetLastApiInfo().RateLimit;

                return _mapper.Map<RateLimit>(rateLimit);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message, ex);
            }
        }

        private async Task Execute(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (Octokit.ForbiddenException ex)
            {
                throw new ForbiddenException(ex.Message, ex);
            }
            catch (Octokit.NotFoundException ex)
            {
                throw new NotFoundException(ex.Message, ex);
            }
            catch (Exception ex) when (!(ex is NotFoundException))
            {
                throw new ApiException(ex.Message, ex);
            }
        }

        private async Task<T> Execute<T>(Func<Task<T>> action)
        {
            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (Octokit.ForbiddenException ex)
            {
                throw new ForbiddenException(ex.Message, ex);
            }
            catch (Octokit.NotFoundException ex)
            {
                throw new NotFoundException(ex.Message, ex);
            }
            catch (Exception ex) when (!(ex is NotFoundException))
            {
                throw new ApiException(ex.Message, ex);
            }
        }

        private ApiOptions GetApiOptions(int startPage = 1, int pageSize = 100, int pageCount = 1)
        {
            return new ApiOptions
            {
                StartPage = startPage,
                PageSize = pageSize,
                PageCount = pageCount,
            };
        }
    }
}