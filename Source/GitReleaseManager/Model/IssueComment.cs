// -----------------------------------------------------------------------
// <copyright file="IssueComment.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Model
{
    public class IssueComment
    {
        /// <summary>
        /// The issue comment Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Details about the issue comment.
        /// </summary>
        public string Body { get; set; }
    }
}