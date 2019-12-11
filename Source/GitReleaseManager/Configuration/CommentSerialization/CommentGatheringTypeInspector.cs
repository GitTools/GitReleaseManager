// -----------------------------------------------------------------------
// <copyright file="CommentGatheringTypeInspector.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------
// All of the classes in this file have been aquired from
// https://dotnetfiddle.net/8M6iIE which was mentioned
// on the YamlDotNet repository here: https://github.com/aaubry/YamlDotNet/issues/444#issuecomment-546709672

namespace GitReleaseManager.Core.Configuration.CommentSerialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.TypeInspectors;

    public sealed class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _innerTypeDescriptor;

        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            _innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException(nameof(innerTypeDescriptor));
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return _innerTypeDescriptor.GetProperties(type, container).Select(d => new CommentsPropertyDescriptor(d));
        }

        private sealed class CommentsPropertyDescriptor : IPropertyDescriptor
        {
            private readonly IPropertyDescriptor _baseDescriptor;

            public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
            {
                _baseDescriptor = baseDescriptor;
                Name = baseDescriptor.Name;
            }

            public string Name { get; set; }

            public Type Type => _baseDescriptor.Type;

            public Type TypeOverride
            {
                get => _baseDescriptor.TypeOverride;
                set => _baseDescriptor.TypeOverride = value;
            }

            public bool CanWrite => _baseDescriptor.CanWrite;

            public int Order { get; set; }

            public ScalarStyle ScalarStyle
            {
                get => _baseDescriptor.ScalarStyle;
                set => _baseDescriptor.ScalarStyle = value;
            }

            public T GetCustomAttribute<T>()
                where T : Attribute
            {
                return _baseDescriptor.GetCustomAttribute<T>();
            }

            public IObjectDescriptor Read(object target)
            {
                var description = _baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
                return description != null
                    ? new CommentsObjectDescriptor(_baseDescriptor.Read(target), description.Description)
                    : _baseDescriptor.Read(target);
            }

            public void Write(object target, object value)
            {
                _baseDescriptor.Write(target, value);
            }
        }
    }
}