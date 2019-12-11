// -----------------------------------------------------------------------
// <copyright file="GitHubProfile.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core.MappingProfiles
{
    using AutoMapper;

    public class GitHubProfile : Profile
    {
        public GitHubProfile()
        {
            CreateMap<Model.Issue, Octokit.Issue>().ReverseMap();
            CreateMap<Model.Release, Octokit.Release>().ReverseMap();
            CreateMap<Model.Label, Octokit.Label>().ReverseMap();
            CreateMap<Model.Milestone, Octokit.Milestone>();
            CreateMap<Octokit.Milestone, Model.Milestone>()
                .AfterMap((src, dest) => dest.Version = src.Version());
        }
    }
}
