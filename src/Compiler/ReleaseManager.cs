//-----------------------------------------------------------------------
// <copyright file="ReleaseManager.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ReleaseNotesCompiler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Octokit;

    public class ReleaseManager
    {
        private GitHubClient gitHubClient;
        private string organization;

        public ReleaseManager(GitHubClient gitHubClient, string organization)
        {
            this.gitHubClient = gitHubClient;
            this.organization = organization;
        }

        public async Task<List<ReleaseUpdateRequired>> GetReleasesInNeedOfUpdates()
        {
            var repositories = await this.gitHubClient.Repository.GetAllForOrg(this.organization);

            var releases = new List<ReleaseUpdateRequired>();

            foreach (var repository in repositories.Where(r =>
                r.Name != "NServiceBus" &&  // until we can patch octokit
                r.Name != "ServiceInsight" && // until we can patch octokit
                r.HasIssues))
            {
                Console.Out.WriteLine("Checking " + repository.Name);

                var milestones = await this.GetMilestones(repository.Name);

                var releasesForThisRepo = await this.gitHubClient.Release.GetAll(this.organization, repository.Name);

                foreach (var milestone in milestones)
                {
                    var potentialRelease = milestone.Title.Replace(" ", string.Empty);

                    var release = releasesForThisRepo.SingleOrDefault(r => r.Name == potentialRelease);

                    if (release != null)
                    {
                        var releaseUpdatedAt = this.GetUpdatedAt(release).ToUniversalTime();

                        var allIssues = await this.gitHubClient.AllIssuesForMilestone(milestone);

                        var latestIssueModification = allIssues.Where(i => i.State == ItemState.Closed).Max(i => i.ClosedAt.Value).UtcDateTime;

                        Console.Out.WriteLine("Release exists for milestone {0} - Last updated at: {1}, Issues updated at:{2}", potentialRelease, releaseUpdatedAt, latestIssueModification);

                        if (releaseUpdatedAt < latestIssueModification)
                        {
                            releases.Add(new ReleaseUpdateRequired
                            {
                                Release = release.Name,
                                Repository = repository.Name,
                                NeedsToBeCreated = true
                            });
                        }
                    }
                    else
                    {
                        Console.Out.WriteLine("No Release exists for milestone " + potentialRelease);

                        releases.Add(new ReleaseUpdateRequired
                        {
                            Release = potentialRelease,
                            Repository = repository.Name,
                            NeedsToBeCreated = true
                        });
                    }
                }
            }

            return releases;
        }

        private DateTime GetUpdatedAt(Release release)
        {
            // we try to parse our footer
            var temp = release.Body.Split(new[] { " at " }, StringSplitOptions.RemoveEmptyEntries);

            if (!temp.Any())
            {
                return DateTime.MinValue;
            }

            DateTime result;

            if (!DateTime.TryParse(temp.Last().Split(' ').First(), out result))
            {
                return DateTime.MinValue;
            }

            return result;
        }

        private async Task<List<Milestone>> GetMilestones(string repository)
        {
            var milestonesClient = this.gitHubClient.Issue.Milestone;
            var openList = await milestonesClient.GetForRepository(this.organization, repository, new MilestoneRequest { State = ItemState.Open });

            return openList.ToList();
        }
    }
}