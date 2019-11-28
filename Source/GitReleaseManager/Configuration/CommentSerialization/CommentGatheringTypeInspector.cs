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
        private readonly ITypeInspector innerTypeDescriptor;

        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            if (innerTypeDescriptor == null)
            {
                throw new ArgumentNullException(nameof(innerTypeDescriptor));
            }

            this.innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return this.innerTypeDescriptor.GetProperties(type, container).Select(d => new CommentsPropertyDescriptor(d));
        }

        private sealed class CommentsPropertyDescriptor : IPropertyDescriptor
        {
            private readonly IPropertyDescriptor baseDescriptor;

            public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
            {
                this.baseDescriptor = baseDescriptor;
                Name = baseDescriptor.Name;
            }

            public string Name { get; set; }

            public Type Type => this.baseDescriptor.Type;

            public Type TypeOverride
            {
                get => this.baseDescriptor.TypeOverride;
                set => this.baseDescriptor.TypeOverride = value;
            }

            public bool CanWrite => this.baseDescriptor.CanWrite;

            public int Order { get; set; }

            public ScalarStyle ScalarStyle
            {
                get => this.baseDescriptor.ScalarStyle;
                set => this.baseDescriptor.ScalarStyle = value;
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return this.baseDescriptor.GetCustomAttribute<T>();
            }

            public IObjectDescriptor Read(object target)
            {
                var description = this.baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
                return description != null
                    ? new CommentsObjectDescriptor(this.baseDescriptor.Read(target), description.Description)
                    : this.baseDescriptor.Read(target);
            }

            public void Write(object target, object value)
            {
                this.baseDescriptor.Write(target, value);
            }
        }
    }
}