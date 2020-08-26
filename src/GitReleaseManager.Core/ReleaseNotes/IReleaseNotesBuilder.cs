//-----------------------------------------------------------------------
// <copyright file="IReleaseNotesBuilder.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

namespace GitReleaseManager.Core.ReleaseNotes
{
    public interface IReleaseNotesBuilder
    {
        Task<string> BuildReleaseNotes(string user, string repository, string milestoneTitle, string templateText);
    }
}