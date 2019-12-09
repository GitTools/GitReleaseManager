using System.Collections.Generic;

namespace GitReleaseManager.Core.Model
{
    public sealed class Issue
    {
        public string Title { get; set; }
        public string Number { get; set; }
        public string HtmlUrl { get; set; }
        public IReadOnlyList<Label> Labels { get; set; }
    }
}