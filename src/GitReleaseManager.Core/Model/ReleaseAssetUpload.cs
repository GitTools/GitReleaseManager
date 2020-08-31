// -----------------------------------------------------------------------
// <copyright file="ReleaseAssetUpload.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Model
{
    using System.IO;

    public class ReleaseAssetUpload
    {
        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the raw data.
        /// </summary>
        /// <value>The raw data.</value>
        public Stream RawData { get; set; }
    }
}