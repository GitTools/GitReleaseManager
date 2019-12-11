// -----------------------------------------------------------------------
// <copyright file="CommentsObjectDescriptor.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------
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
        private readonly IObjectDescriptor _innerDescriptor;

        public CommentsObjectDescriptor(IObjectDescriptor innerDescriptor, string comment)
        {
            _innerDescriptor = innerDescriptor;
            Comment = comment;
        }

        public string Comment { get; private set; }

        public object Value => _innerDescriptor.Value;

        public Type Type => _innerDescriptor.Type;

        public Type StaticType => _innerDescriptor.StaticType;

        public ScalarStyle ScalarStyle => _innerDescriptor.ScalarStyle;
    }
}