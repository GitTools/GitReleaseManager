// -----------------------------------------------------------------------
// <copyright file="Label.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Model
{
    public sealed class Label
    {
        /// <summary>
        /// Gets or sets the name of the label (required).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the color of the label (required).
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets a short description of the label (optional).
        /// </summary>
        public string Description { get; set; }
    }
}