// -----------------------------------------------------------------------
// <copyright file="NotFoundException.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}