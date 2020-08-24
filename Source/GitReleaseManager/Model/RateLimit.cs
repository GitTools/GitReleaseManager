// -----------------------------------------------------------------------
// <copyright file="RateLimit.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace GitReleaseManager.Core.Model
{
    public class RateLimit
    {
        /// <summary>
        /// The maximum number of requests that the consumer is permitted to make per hour.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// The number of requests remaining in the current rate limit window.
        /// </summary>
        public int Remaining { get; set; }

        /// <summary>
        /// The date and time at which the current rate limit window resets.
        /// </summary>
        public DateTimeOffset Reset { get; set; }
    }
}