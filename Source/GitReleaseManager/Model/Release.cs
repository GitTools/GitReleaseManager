// -----------------------------------------------------------------------
// <copyright file="Release.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Model
{
    using System;

    public sealed class Release
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public string TagName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string HtmlUrl { get; set; }

        public bool Draft { get; set; }
    }
}