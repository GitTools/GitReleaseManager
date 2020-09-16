//-----------------------------------------------------------------------
// <copyright file="IReleaseNotesBuilder.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Core.ReleaseNotes
{
    using System.Threading.Tasks;

    public interface IReleaseNotesBuilder
    {
        Task<string> BuildReleaseNotes(string user, string repository, string milestoneTitle, string template);
    }
}