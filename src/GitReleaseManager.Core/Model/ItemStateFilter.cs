// -----------------------------------------------------------------------
// <copyright file="ItemStateFilter.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Model
{
    public enum ItemStateFilter
    {
        /// <summary>
        /// Items that are open.
        /// </summary>
        Open = 0,

        /// <summary>
        /// Items that are closed.
        /// </summary>
        Closed = 1,

        /// <summary>
        /// All the items.
        /// </summary>
        All = 2,
    }
}