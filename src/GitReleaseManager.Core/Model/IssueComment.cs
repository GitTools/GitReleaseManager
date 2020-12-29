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
        /// Gets or sets the issue comment Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets details about the issue comment.
        /// </summary>
        public string Body { get; set; }
    }
}