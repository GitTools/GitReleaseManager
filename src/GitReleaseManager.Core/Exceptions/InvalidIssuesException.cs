using System;
using System.Collections.Generic;

namespace GitReleaseManager.Core.Exceptions
{
    [Serializable]
    public class InvalidIssuesException : Exception
    {
        public InvalidIssuesException()
        {
        }

        public InvalidIssuesException(List<string> errors)
            : base(string.Join(Environment.NewLine, errors))
        {
            Errors = errors;
        }

        public InvalidIssuesException(List<string> errors, string message)
            : base(message)
        {
            Errors = errors;
        }

        public InvalidIssuesException(List<string> errors, string message, Exception inner)
            : base(message, inner)
        {
            Errors = errors;
        }

        public List<string> Errors { get; set; }

        protected InvalidIssuesException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
