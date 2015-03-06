//-----------------------------------------------------------------------
// <copyright file="ReleaseUpdateRequired.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ReleaseNotesCompiler
{
    public class ReleaseUpdateRequired
    {
        public string Repository { get; set; }

        public string Release { get; set; }

        public bool NeedsToBeCreated { get; set; }

        public override string ToString()
        {
            return string.Format("Update required for release {0}(Repo: {1}), NeedsToBeCreated: {2}", this.Release, this.Repository, this.NeedsToBeCreated);
        }
    }
}