//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilder.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Configuration;
    using Octokit;

    public class ReleaseNotesBuilder
    {
        private IGitHubClient gitHubClient;
        private string user;
        private string repository;
        private string milestoneTitle;
        private ReadOnlyCollection<Milestone> milestones;
        private Milestone targetMilestone;
        private Config configuration;

        public ReleaseNotesBuilder(IGitHubClient gitHubClient, string user, string repository, string milestoneTitle, Config configuration)
        {
            this.gitHubClient = gitHubClient;
            this.user = user;
            this.repository = repository;
            this.milestoneTitle = milestoneTitle;
            this.configuration = configuration;
        }

        public async Task<string> BuildReleaseNotes()
        {
            this.LoadMilestones();
            this.GetTargetMilestone();

            var issues = await this.GetIssues(this.targetMilestone);
            var stringBuilder = new StringBuilder();
            var previousMilestone = this.GetPreviousMilestone();
            var numberOfCommits = await this.gitHubClient.GetNumberOfCommitsBetween(previousMilestone, this.targetMilestone);

            if (issues.Count > 0)
            {
                var issuesText = string.Format(issues.Count == 1 ? "{0} issue" : "{0} issues", issues.Count);

                if (numberOfCommits > 0)
                {
                    var commitsLink = this.GetCommitsLink(previousMilestone);
                    var commitsText = string.Format(numberOfCommits == 1 ? "{0} commit" : "{0} commits", numberOfCommits);

                    stringBuilder.AppendFormat(@"As part of this release we had [{0}]({1}) which resulted in [{2}]({3}) being closed.", commitsText, commitsLink, issuesText, this.targetMilestone.HtmlUrl());
                }
                else
                {
                    stringBuilder.AppendFormat(@"As part of this release we had [{0}]({1}) closed.", issuesText, this.targetMilestone.HtmlUrl());
                }
            }
            else if (numberOfCommits > 0)
            {
                var commitsLink = this.GetCommitsLink(previousMilestone);
                var commitsText = string.Format(numberOfCommits == 1 ? "{0} commit" : "{0} commits", numberOfCommits);
                stringBuilder.AppendFormat(@"As part of this release we had [{0}]({1}).", commitsText, commitsLink);
            }

            stringBuilder.AppendLine();

            stringBuilder.AppendLine(this.targetMilestone.Description);
            stringBuilder.AppendLine();

            this.AddIssues(stringBuilder, issues);

            if (this.configuration.Create.IncludeFooter)
            {
                this.AddFooter(stringBuilder);
            }

            return stringBuilder.ToString();
        }

        private void Append(IEnumerable<Issue> issues, string label, StringBuilder stringBuilder)
        {
            var features = issues.Where(x => x.Labels.Any(l => l.Name == label)).ToList();

            if (features.Count > 0)
            {
                var singular = this.GetLabel(label, alias => alias.Header) ?? label;
                var plural = this.GetLabel(label, alias => alias.Plural) ?? label + "s";
                stringBuilder.AppendFormat("__{0}__\r\n\r\n", features.Count == 1 ? singular : plural);

                foreach (var issue in features)
                {
                    stringBuilder.AppendFormat("- [__#{0}__]({1}) {2}\r\n", issue.Number, issue.HtmlUrl, issue.Title);
                }

                stringBuilder.AppendLine();
            }
        }

        private string GetLabel(string label, Func<LabelAlias, string> func)
        {
            var alias = this.configuration.LabelAliases.FirstOrDefault(x => x.Name.Equals(label, StringComparison.OrdinalIgnoreCase));
            return alias != null ? func(alias) : null;
        }

        private void CheckForValidLabels(Issue issue)
        {
            var count = 0;

            foreach (var issueLabel in issue.Labels)
            {
                count += this.configuration.IssueLabelsInclude.Count(issueToInclude => issueLabel.Name.ToUpperInvariant() == issueToInclude.ToUpperInvariant());

                count += this.configuration.IssueLabelsExclude.Count(issueToExclude => issueLabel.Name.ToUpperInvariant() == issueToExclude.ToUpperInvariant());
            }

            if (count != 1)
            {
                var allIssueLabels = this.configuration.IssueLabelsInclude.Union(this.configuration.IssueLabelsExclude).ToList();
                var allIssuesExceptLast = allIssueLabels.Take(allIssueLabels.Count - 1);
                var lastLabel = allIssueLabels.Last();

                var allIssuesExceptLastString = string.Join(", ", allIssuesExceptLast);

                var message = string.Format(CultureInfo.InvariantCulture, "Bad Issue {0} expected to find a single label with either {1} or {2}.", issue.HtmlUrl, allIssuesExceptLastString, lastLabel);
                throw new InvalidOperationException(message);
            }
        }

        private void AddIssues(StringBuilder stringBuilder, List<Issue> issues)
        {
            foreach (var issueLabel in this.configuration.IssueLabelsInclude)
            {
                this.Append(issues, issueLabel, stringBuilder);
            }
        }

        private Milestone GetPreviousMilestone()
        {
            var currentVersion = this.targetMilestone.Version();
            return this.milestones
                .OrderByDescending(m => m.Version())
                .Distinct().ToList()
                .SkipWhile(x => x.Version() >= currentVersion)
                .FirstOrDefault();
        }

        private string GetCommitsLink(Milestone previousMilestone)
        {
            if (previousMilestone == null)
            {
                return string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/commits/{2}", this.user, this.repository, this.targetMilestone.Title);
            }

            return string.Format(CultureInfo.InvariantCulture, "https://github.com/{0}/{1}/compare/{2}...{3}", this.user, this.repository, previousMilestone.Title, this.targetMilestone.Title);
        }

        private void AddFooter(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "### {0}", this.configuration.Create.FooterHeading));

            var footerContent = this.configuration.Create.FooterContent;

            if (this.configuration.Create.FooterIncludesMilestone)
            {
                if (!string.IsNullOrEmpty(this.configuration.Create.MilestoneReplaceText))
                {
                    footerContent = footerContent.Replace(this.configuration.Create.MilestoneReplaceText, this.milestoneTitle);
                }
            }

            stringBuilder.Append(footerContent);
        }

        private void LoadMilestones()
        {
            this.milestones = this.gitHubClient.GetMilestones();
        }

        private async Task<List<Issue>> GetIssues(Milestone milestone)
        {
            var issues = await this.gitHubClient.GetIssues(milestone);
            foreach (var issue in issues)
            {
                this.CheckForValidLabels(issue);
            }

            return issues;
        }

        private void GetTargetMilestone()
        {
            this.targetMilestone = this.milestones.FirstOrDefault(x => x.Title == this.milestoneTitle);

            if (this.targetMilestone == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not find milestone for '{0}'.", this.milestoneTitle));
            }
        }
    }
}