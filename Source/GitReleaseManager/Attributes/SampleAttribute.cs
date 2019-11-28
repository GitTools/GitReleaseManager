namespace GitReleaseManager.Core.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SampleAttribute : Attribute
    {
        public SampleAttribute(object value)
        {
            this.Value = value;
        }

        public object Value { get; private set; }
    }
}