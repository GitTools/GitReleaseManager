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
        IGitHubClient gitHubClient;
        string user;
        string repository;
        string milestoneTitle;
        List<Milestone> milestones;
        Milestone targetMilestone;

        public ReleaseNotesBuilder(IGitHubClient gitHubClient, string user, string repository, string milestoneTitle)
        {
            this.gitHubClient = gitHubClient;
            this.user = user;
            this.repository = repository;
            this.milestoneTitle = milestoneTitle;
        }

        public async Task<string> BuildReleaseNotes()
        {
            LoadMilestones();

            GetTargetMilestone();
            var issues = await GetIssues(targetMilestone);
            var stringBuilder = new StringBuilder();
            var previousMilestone = GetPreviousMilestone();
            var numberOfCommits = await gitHubClient.GetNumberOfCommitsBetween(previousMilestone, targetMilestone);

            if (issues.Count > 0)
            {
                var issuesText = String.Format(issues.Count == 1 ? "{0} issue" : "{0} issues", issues.Count);

                if (numberOfCommits > 0)
                {
                    var commitsLink = GetCommitsLink(previousMilestone);
                    var commitsText = String.Format(numberOfCommits == 1 ? "{0} commit" : "{0} commits", numberOfCommits);

                    stringBuilder.AppendFormat(@"As part of this release we had [{0}]({1}) which resulted in [{2}]({3}) being closed.", commitsText, commitsLink, issuesText, targetMilestone.HtmlUrl());
                }
                else
                {
                    stringBuilder.AppendFormat(@"As part of this release we had [{0}]({1}) closed.", issuesText, targetMilestone.HtmlUrl());
                }
            }
            else if (numberOfCommits > 0)
            {
                var commitsLink = GetCommitsLink(previousMilestone);
                var commitsText = String.Format(numberOfCommits == 1 ? "{0} commit" : "{0} commits", numberOfCommits);
                stringBuilder.AppendFormat(@"As part of this release we had [{0}]({1}).", commitsText, commitsLink);
            }
            stringBuilder.AppendLine();

            stringBuilder.AppendLine(targetMilestone.Description);
            stringBuilder.AppendLine();

            AddIssues(stringBuilder, issues);

            await AddFooter(stringBuilder);

            return stringBuilder.ToString();
        }

        Milestone GetPreviousMilestone()
        {
            var currentVersion = targetMilestone.Version();
            return milestones
                .OrderByDescending(m => m.Version())
                .Distinct().ToList()
                .SkipWhile(x => x.Version() >= currentVersion)
                .FirstOrDefault();
        }

        string GetCommitsLink(Milestone previousMilestone)
        {
            if (previousMilestone == null)
            {
                return string.Format("https://github.com/{0}/{1}/commits/{2}", user, repository, targetMilestone.Title);
            }
            return string.Format("https://github.com/{0}/{1}/compare/{2}...{3}", user, repository, previousMilestone.Title, targetMilestone.Title);
        }

        void AddIssues(StringBuilder stringBuilder, List<Issue> issues)
        {
            Append(issues, "Feature", stringBuilder);
            Append(issues, "Improvement", stringBuilder);
            Append(issues, "Bug", stringBuilder);
        }

        static async Task AddFooter(StringBuilder stringBuilder)
        {
            var file = new FileInfo("footer.md");

            if (!file.Exists)
            {
                file = new FileInfo("footer.txt");
            }

            if (!file.Exists)
            {
                stringBuilder.Append(@"## Where to get it
You can download this release from [chocolatey](https://chocolatey.org/packages/ChocolateyGUI)");
                return;
            }

            using (var reader = file.OpenText())
            {
                stringBuilder.Append(await reader.ReadToEndAsync());
            }
        }

        void LoadMilestones()
        {
            milestones = gitHubClient.GetMilestones();
        }

        async Task<List<Issue>> GetIssues(Milestone milestone)
        {
            var issues = await gitHubClient.GetIssues(milestone);
            foreach (var issue in issues)
            {
                CheckForValidLabels(issue);
            }
            return issues;
        }

        static void CheckForValidLabels(Issue issue)
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

        void Append(IEnumerable<Issue> issues, string label, StringBuilder stringBuilder)
        {
            var features = issues.Where(x => x.Labels.Any(l => l.Name == label))
                .ToList();
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

        void GetTargetMilestone()
        {
            targetMilestone = milestones.FirstOrDefault(x => x.Title == milestoneTitle);
            if (targetMilestone == null)
            {
                throw new Exception(string.Format("Could not find milestone for '{0}'.", milestoneTitle));
            }
        }
    }
}