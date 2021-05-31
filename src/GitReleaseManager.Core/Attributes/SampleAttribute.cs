using System;

namespace GitReleaseManager.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SampleAttribute : Attribute
    {
        public SampleAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }
    }
}