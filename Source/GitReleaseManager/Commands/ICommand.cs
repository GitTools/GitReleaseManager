// -----------------------------------------------------------------------
// <copyright file="ICommand.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;
using GitReleaseManager.Core.Options;

namespace GitReleaseManager.Core.Commands
{
    public interface ICommand<TOptions>
        where TOptions : BaseSubOptions
    {
        Task<int> Execute(TOptions options);
    }
}