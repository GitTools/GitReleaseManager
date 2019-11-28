// All of the classes in this file have been aquired from
// https://dotnetfiddle.net/8M6iIE which was mentioned
// on the YamlDotNet repository here: https://github.com/aaubry/YamlDotNet/issues/444#issuecomment-546709672

namespace GitReleaseManager.Core.Configuration.CommentSerialization
{
    using System;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    public sealed class CommentsObjectDescriptor : IObjectDescriptor
    {
        private readonly IObjectDescriptor innerDescriptor;

        public CommentsObjectDescriptor(IObjectDescriptor innerDescriptor, string comment)
        {
            this.innerDescriptor = innerDescriptor;
            Comment = comment;
        }

        public string Comment { get; private set; }

        public object Value => this.innerDescriptor.Value;
        public Type Type => this.innerDescriptor.Type;
        public Type StaticType => this.innerDescriptor.StaticType;
        public ScalarStyle ScalarStyle => this.innerDescriptor.ScalarStyle;
    }
}