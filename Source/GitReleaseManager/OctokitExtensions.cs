//-----------------------------------------------------------------------
// <copyright file="OctokitExtensions.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Octokit;
    using Serilog;

    public static class OctokitExtensions
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(OctokitExtensions));

        public static bool IsPullRequest(this Issue issue)
        {
            if (issue is null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            return !(issue.PullRequest is null);
        }

        public static async Task<IEnumerable<Issue>> AllIssuesForMilestone(this GitHubClient gitHubClient, Milestone milestone)
        {
            if (gitHubClient is null)
            {
                throw new ArgumentNullException(nameof(gitHubClient));
            }

            if (milestone is null)
            {
                throw new ArgumentNullException(nameof(milestone));
            }

            var closedIssueRequest = new RepositoryIssueRequest
            {
                Milestone = milestone.Number.ToString(CultureInfo.InvariantCulture),
                State = ItemStateFilter.Closed,
            };

            var openIssueRequest = new RepositoryIssueRequest
            {
                Milestone = milestone.Number.ToString(CultureInfo.InvariantCulture),
                State = ItemStateFilter.Open,
            };

            var parts = milestone.Url.Split('/');
            var user = parts[4];
            var repository = parts[5];

            _logger.Verbose("Finding closed issues for milestone {Milestone} on {Owner}/{Repository}", milestone.Title, user, repository);
            var closedIssues = await gitHubClient.Issue.GetAllForRepository(user, repository, closedIssueRequest).ConfigureAwait(false);
            _logger.Verbose("Finding open issues for milestone {Milestone} on {Owner}/{Repository}", milestone.Title, user, repository);
            var openIssues = await gitHubClient.Issue.GetAllForRepository(user, repository, openIssueRequest).ConfigureAwait(false);

            return openIssues.Union(closedIssues);
        }

        public static Uri HtmlUrl(this Milestone milestone)
        {
            if (milestone is null)
            {
                throw new ArgumentNullException(nameof(milestone));
            }

            var parts = milestone.Url.Split('/');
            var user = parts[2];
            var repository = parts[3];

            return new Uri(string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/issues?milestone={2}&state=closed", user, repository, milestone.Number));
        }
    }
}