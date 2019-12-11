// -----------------------------------------------------------------------
// <copyright file="Issue.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Model
{
    using System.Collections.Generic;

    public sealed class Issue
    {
        public string Title { get; set; }

        public string Number { get; set; }

        public string HtmlUrl { get; set; }

        public IReadOnlyList<Label> Labels { get; set; }
    }
}