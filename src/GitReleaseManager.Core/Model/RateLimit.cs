// -----------------------------------------------------------------------
// <copyright file="RateLimit.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Model
{
    using System;

    public class RateLimit
    {
        /// <summary>
        /// Gets or sets the maximum number of requests that the consumer is permitted to make per hour.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the number of requests remaining in the current rate limit window.
        /// </summary>
        public int Remaining { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the current rate limit window resets.
        /// </summary>
        public DateTimeOffset Reset { get; set; }
    }
}