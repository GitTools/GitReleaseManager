using System.Collections.Generic;

namespace GitReleaseManager.Core.Model
{
    public sealed class Issue
    {
        public string Title { get; set; }

        public int InternalNumber { get; set; }

        public int PublicNumber { get; set; }

        public string HtmlUrl { get; set; }

        public IReadOnlyList<Label> Labels { get; set; }

        public bool IsPullRequest { get; set; }
    }
}