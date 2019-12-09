using System;

namespace GitReleaseManager.Core.Model
{
    public sealed class Release
    {
        public string Body { get; set; }
        public string TagName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string HtmlUrl { get; set; }
    }
}