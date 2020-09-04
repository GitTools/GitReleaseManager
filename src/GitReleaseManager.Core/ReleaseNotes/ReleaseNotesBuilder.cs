//-----------------------------------------------------------------------
// <copyright file="ReleaseNotesBuilder.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.ReleaseNotes
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Configuration;
    using GitReleaseManager.Core.Extensions;
    using GitReleaseManager.Core.Model;
    using GitReleaseManager.Core.Provider;
    using Scriban;
    using Serilog;

    public class ReleaseNotesBuilder : IReleaseNotesBuilder
    {
        private readonly IVcsProvider _vcsProvider;
        private readonly ILogger _logger;
        private readonly Config _configuration;
        private string _user;
        private string _repository;
        private string _milestoneTitle;
        private IEnumerable<Milestone> _milestones;
        private Milestone _targetMilestone;

        public ReleaseNotesBuilder(IVcsProvider vcsProvider, ILogger logger, Config configuration)
        {
            _vcsProvider = vcsProvider;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> BuildReleaseNotes(string user, string repository, string milestoneTitle, string templateText)
        {
            _user = user;
            _repository = repository;
            _milestoneTitle = milestoneTitle;

            _logger.Verbose("Building release notes...");
            await LoadMilestonesAsync().ConfigureAwait(false);
            GetTargetMilestone();

            var issues = await GetIssuesAsync(_targetMilestone).ConfigureAwait(false);
            var previousMilestone = GetPreviousMilestone();

            var @base = previousMilestone != null
                ? previousMilestone.Title
                : _configuration.DefaultBranch;

            var head = _targetMilestone.Title;

            _logger.Verbose("Getting commit count between base '{Base}' and head '{Head}'", @base, head);

            var numberOfCommits = await _vcsProvider.GetCommitsCount(_user, _repository, @base, head).ConfigureAwait(false);

            if (issues.Count == 0)
            {
                var logMessage = string.Format("No closed issues have been found for milestone {0}, or all assigned issues are meant to be excluded from release notes, aborting creation of release.", _milestoneTitle);
                throw new InvalidOperationException(logMessage);
            }

            var commitsLink = _vcsProvider.GetCommitsUrl(_user, _repository, _targetMilestone?.Title, previousMilestone?.Title);
            var commitsText = string.Format(numberOfCommits == 1 ? "{0} commit" : "{0} commits", numberOfCommits);
            var issuesText = string.Format(issues.Count == 1 ? "{0} issue" : "{0} issues", issues.Count);

            var footerContent = _configuration.Create.FooterContent;

            if (_configuration.Create.FooterIncludesMilestone &&
                !string.IsNullOrEmpty(_configuration.Create.MilestoneReplaceText))
            {
                var replaceValues = new Dictionary<string, object>
                {
                    { _configuration.Create.MilestoneReplaceText.Trim('{', '}'), _milestoneTitle },
                };
                footerContent = footerContent.ReplaceTemplate(replaceValues);
            }

            var issuesDict = GetIssuesDict(issues);

            var templateContext = new
            {
                IssuesCount = issues.Count,
                CommitsCount = numberOfCommits,
                CommitsLink = commitsLink,
                CommitsText = commitsText,
                IssuesText = issuesText,
                MilestoneDescription = _targetMilestone.Description,
                MilestoneHtmlUrl = _targetMilestone.HtmlUrl,
                IssueLabels = issuesDict.Keys.ToList(),
                Issues = issuesDict,
                IncludeFooter = _configuration.Create.IncludeFooter,
                FooterHeading = _configuration.Create.FooterHeading,
                FooterContent = footerContent,
            };

            var template = Template.Parse(templateText);
            var releaseNotes = template.Render(templateContext);

            _logger.Verbose("Finished building release notes");

            return releaseNotes;
        }

        private Dictionary<string, List<Issue>> GetIssuesDict(List<Issue> issues)
        {
            var issueLabels = _configuration.IssueLabelsInclude;
            var issuesByLabel = issues
                .SelectMany(o => o.Labels, (issue, label) => new { Label = label.Name, Issue = issue })
                .Where(o => issueLabels.Contains(o.Label))
                .GroupBy(o => o.Label, o => o.Issue)
                .OrderBy(o => o.Key)
                .ToDictionary(o => GetValidLabel(o.Key, o.Count()), o => o.OrderBy(issue => issue.Number).ToList());

            return issuesByLabel;
        }

        private string GetValidLabel(string label, int issuesCount)
        {
            var alias = _configuration.LabelAliases.FirstOrDefault(x => x.Name.Equals(label, StringComparison.OrdinalIgnoreCase));
            var newLabel = label;

            if (alias != null)
            {
                newLabel = issuesCount == 1 ? alias.Header : alias.Plural;
            }
            else if (issuesCount > 1)
            {
                newLabel += "s";
            }

            return newLabel;
        }

        private bool CheckForValidLabels(Issue issue)
        {
            var includedIssuesCount = 0;
            var excludedIssuesCount = 0;

            foreach (var issueLabel in issue.Labels)
            {
                includedIssuesCount += _configuration.IssueLabelsInclude.Count(issueToInclude => issueLabel.Name.ToUpperInvariant() == issueToInclude.ToUpperInvariant());

                excludedIssuesCount += _configuration.IssueLabelsExclude.Count(issueToExclude => issueLabel.Name.ToUpperInvariant() == issueToExclude.ToUpperInvariant());
            }

            if (includedIssuesCount + excludedIssuesCount != 1)
            {
                var allIssueLabels = _configuration.IssueLabelsInclude.Union(_configuration.IssueLabelsExclude).ToList();
                var allIssuesExceptLast = allIssueLabels.Take(allIssueLabels.Count - 1);
                var lastLabel = allIssueLabels.Last();

                var allIssuesExceptLastString = string.Join(", ", allIssuesExceptLast);

                var message = string.Format(CultureInfo.InvariantCulture, "Bad Issue {0} expected to find a single label with either {1} or {2}.", issue.HtmlUrl, allIssuesExceptLastString, lastLabel);
                throw new InvalidOperationException(message);
            }

            if (includedIssuesCount > 0)
            {
                return true;
            }

            return false;
        }

        private Milestone GetPreviousMilestone()
        {
            var currentVersion = _targetMilestone.Version;
            return _milestones
                .OrderByDescending(m => m.Version)
                .Distinct()
                .SkipWhile(x => x.Version >= currentVersion)
                .FirstOrDefault();
        }

        private async Task LoadMilestonesAsync()
        {
            _milestones = await _vcsProvider.GetMilestonesAsync(_user, _repository).ConfigureAwait(false);
        }

        private async Task<List<Issue>> GetIssuesAsync(Milestone milestone)
        {
            var issues = await _vcsProvider.GetIssuesAsync(_user, _repository, milestone.Number, ItemStateFilter.Closed).ConfigureAwait(false);

            var hasIncludedIssues = false;

            foreach (var issue in issues)
            {
                if (CheckForValidLabels(issue))
                {
                    hasIncludedIssues = true;
                }
            }

            // If there are no issues assigned to the milestone that have a label that is part
            // of the labels to include array, then that is essentially the same as having no
            // closed issues assigned to the milestone.  In this scenario, we want to raise an
            // error, so return an emtpy issues list.
            if (!hasIncludedIssues)
            {
                return new List<Issue>();
            }

            return issues.ToList();
        }

        private void GetTargetMilestone()
        {
            _targetMilestone = _milestones.FirstOrDefault(x => x.Title == _milestoneTitle);

            if (_targetMilestone == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not find milestone for '{0}'.", _milestoneTitle));
            }
        }
    }
}