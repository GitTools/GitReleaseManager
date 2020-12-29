// -----------------------------------------------------------------------
// <copyright file="ICommand.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.Commands
{
    using System.Threading.Tasks;
    using GitReleaseManager.Core.Options;

    public interface ICommand<TOptions>
        where TOptions : BaseSubOptions
    {
        Task<int> Execute(TOptions options);
    }
}