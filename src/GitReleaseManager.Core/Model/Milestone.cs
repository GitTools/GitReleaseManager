namespace GitReleaseManager.Core.Model
{
    using System;

    public sealed class Milestone
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public int Number { get; set; }

        public string HtmlUrl { get; set; }

        public string Url { get; set; }

        public Version Version { get; set; }
    }
}