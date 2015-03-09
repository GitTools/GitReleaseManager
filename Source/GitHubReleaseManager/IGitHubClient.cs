//-----------------------------------------------------------------------
// <copyright file="IGitHubClient.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Octokit;

    public interface IGitHubClient
    {
        Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone);

        Task<List<Issue>> GetIssues(Milestone targetMilestone);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate")]
        Task<List<Release>> GetReleases();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate")]
        ReadOnlyCollection<Milestone> GetMilestones();
    }
}