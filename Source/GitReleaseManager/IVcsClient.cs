//-----------------------------------------------------------------------
// <copyright file="IVcsClient.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using GitReleaseManager.Core.Model;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    // TODO: Confirm best name for this thing!
    public interface IVcsClient
    {
        Task<int> GetNumberOfCommitsBetween(Milestone previousMilestone, Milestone currentMilestone);

        Task<List<Issue>> GetIssues(Milestone targetMilestone);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate")]
        Task<List<Release>> GetReleases();

        Task<Release> GetSpecificRelease(string tagName);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not appropriate")]
        ReadOnlyCollection<Milestone> GetMilestones();
    }
}