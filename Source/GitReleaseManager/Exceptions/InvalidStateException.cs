// -----------------------------------------------------------------------
// <copyright file="InvalidStateException.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Exceptions
{
    using System;

    [Serializable]
    public class InvalidStateException : Exception
    {
        public InvalidStateException()
        {
        }

        public InvalidStateException(string message)
            : base(message)
        {
        }

        public InvalidStateException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidStateException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}