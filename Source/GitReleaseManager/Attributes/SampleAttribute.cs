// -----------------------------------------------------------------------
// <copyright file="SampleAttribute.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Attributes
{
    using System;

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