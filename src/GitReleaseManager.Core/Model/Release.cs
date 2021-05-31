using System;
using System.Collections.Generic;

namespace GitReleaseManager.Core.Model
{
    public sealed class Release
    {
        public int Id { get; set; }

        public string Body { get; set; }

        public string Name { get; set; }

        public string TagName { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string HtmlUrl { get; set; }

        public string UploadUrl { get; set; }

        public bool Draft { get; set; }

        public bool Prerelease { get; set; }

        public string TargetCommitish { get; set; }

        public IReadOnlyList<ReleaseAsset> Assets { get; set; }
    }
}