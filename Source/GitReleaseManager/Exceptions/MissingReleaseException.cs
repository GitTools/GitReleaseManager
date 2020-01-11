// -----------------------------------------------------------------------
// <copyright file="MissingReleaseException.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Exceptions
{
    using System;

    [Serializable]
    public class MissingReleaseException : Exception
    {
        public MissingReleaseException()
        {
        }

        public MissingReleaseException(string message)
            : base(message)
        {
        }

        public MissingReleaseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected MissingReleaseException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}