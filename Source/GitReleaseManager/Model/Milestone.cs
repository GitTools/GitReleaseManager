// -----------------------------------------------------------------------
// <copyright file="Milestone.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Model
{
    using System;

    public sealed class Milestone
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Number { get; set; }

        public string HtmlUrl { get; set; }

        public string Url { get; set; }

        public Version Version { get; set; }
    }
}