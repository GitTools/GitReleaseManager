//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilder.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ReleaseNotesCompiler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Octokit;

    public class ReleaseNotesBuilder
    {
        private IGitHubClient gitHubClient;
        private string user;
        private string repository;
        private string milestoneTitle;
        private List<Milestone> milestones;
        private Milestone targetMilestone;

        public ReleaseNotesBuilder(IGitHubClient gitHubClient, string user, string repository, string milestoneTitle)
        {
            this.gitHubClient = gitHubClient;
            this.user = user;
            this.repository = repository;
            this.milestoneTitle = milestoneTitle;
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

            await this.AddFooter(stringBuilder);

            return stringBuilder.ToString();
        }

        private static void CheckForValidLabels(Issue issue)
        {
            var count = issue.Labels.Count(l =>
                l.Name == "Bug" ||
                l.Name == "Internal refactoring" ||
                l.Name == "Feature" ||
                l.Name == "Improvement");

            if (count != 1)
            {
                var message = string.Format("Bad Issue {0} expected to find a single label with either 'Bug', 'Internal refactoring', 'Improvement' or 'Feature'.", issue.HtmlUrl);
                throw new Exception(message);
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
                return string.Format("https://github.com/{0}/{1}/commits/{2}", this.user, this.repository, this.targetMilestone.Title);
            }

            return string.Format("https://github.com/{0}/{1}/compare/{2}...{3}", this.user, this.repository, previousMilestone.Title, this.targetMilestone.Title);
        }

        private void AddIssues(StringBuilder stringBuilder, List<Issue> issues)
        {
            this.Append(issues, "Feature", stringBuilder);
            this.Append(issues, "Improvement", stringBuilder);
            this.Append(issues, "Bug", stringBuilder);
        }

        private async Task AddFooter(StringBuilder stringBuilder)
        {
            var file = new FileInfo("footer.md");

            if (!file.Exists)
            {
                file = new FileInfo("footer.txt");
            }

            if (!file.Exists)
            {
                stringBuilder.AppendFormat(@"### Where to get it{0}You can download this release from [chocolatey](https://chocolatey.org/packages/ChocolateyGUI/{1})", Environment.NewLine, this.milestoneTitle);
                return;
            }

            using (var reader = file.OpenText())
            {
                stringBuilder.Append(await reader.ReadToEndAsync());
            }
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
                CheckForValidLabels(issue);
            }

            return issues;
        }

        private void Append(IEnumerable<Issue> issues, string label, StringBuilder stringBuilder)
        {
            var features = issues.Where(x => x.Labels.Any(l => l.Name == label)).ToList();

            if (features.Count > 0)
            {
                stringBuilder.AppendFormat(features.Count == 1 ? "__{0}__\r\n\r\n" : "__{0}s__\r\n\r\n", label);

                foreach (var issue in features)
                {
                    stringBuilder.AppendFormat("- [__#{0}__]({1}) {2}\r\n", issue.Number, issue.HtmlUrl, issue.Title);
                }

                stringBuilder.AppendLine();
            }
        }

        private void GetTargetMilestone()
        {
            this.targetMilestone = this.milestones.FirstOrDefault(x => x.Title == this.milestoneTitle);

            if (this.targetMilestone == null)
            {
                throw new Exception(string.Format("Could not find milestone for '{0}'.", this.milestoneTitle));
            }
        }
    }
}