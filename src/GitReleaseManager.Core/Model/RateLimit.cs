using System;

namespace GitReleaseManager.Core.Model
{
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