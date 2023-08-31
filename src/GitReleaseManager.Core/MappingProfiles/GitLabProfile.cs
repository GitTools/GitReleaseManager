namespace GitReleaseManager.Core.MappingProfiles
{
    using System;
    using AutoMapper;
    using GitReleaseManager.Core.Extensions;

    public class GitLabProfile : Profile
    {
        public GitLabProfile()
        {
            CreateMap<NGitLab.Models.Milestone, Model.Milestone>()
                .ForMember(dest => dest.PublicNumber, act => act.MapFrom(src => src.Iid))
                .ForMember(dest => dest.InternalNumber, act => act.MapFrom(src => src.Id))
                .AfterMap((src, dest) => dest.Version = src.Version());
            CreateMap<NGitLab.Models.ReleaseInfo, Model.Release>()
                .ForMember(dest => dest.Draft, act => act.MapFrom(src => src.ReleasedAt > DateTime.UtcNow))
                .ForMember(dest => dest.Body, act => act.MapFrom(src => src.Description))
                .ForMember(dest => dest.Assets, act => act.MapFrom(src => src.Assets.Links))
                .ReverseMap();
            CreateMap<NGitLab.Models.ReleaseLink, Model.ReleaseAsset>().ReverseMap();
            CreateMap<NGitLab.Models.Issue, Model.Issue>()
                .ForMember(dest => dest.InternalNumber, act => act.MapFrom(src => src.Id))
                .ForMember(dest => dest.PublicNumber, act => act.MapFrom(src => src.IssueId))
                .ForMember(dest => dest.HtmlUrl, act => act.MapFrom(src => src.WebUrl))
                .ForMember(dest => dest.IsPullRequest, act => act.MapFrom(src => false))
                .ReverseMap();
            CreateMap<NGitLab.Models.MergeRequest, Model.Issue>()
                .ForMember(dest => dest.InternalNumber, act => act.MapFrom(src => src.Id))
                .ForMember(dest => dest.PublicNumber, act => act.MapFrom(src => src.Iid))
                .ForMember(dest => dest.HtmlUrl, act => act.MapFrom(src => src.WebUrl))
                .ForMember(dest => dest.IsPullRequest, act => act.MapFrom(src => true))
                .ReverseMap();
            CreateMap<string, Model.Label>().ForMember(dest => dest.Name, act => act.MapFrom(src => src));
            CreateMap<Model.Release, NGitLab.Models.ReleaseCreate>()
                .ForMember(dest => dest.Description, act => act.MapFrom(src => src.Body))
                .ForMember(dest => dest.Ref, act => act.MapFrom(src => src.TargetCommitish))
                .ForMember(dest => dest.Milestones, act => act.MapFrom(src => new string[] { src.TagName }))
                .ForMember(dest => dest.ReleasedAt, act => act.MapFrom(src => src.Draft ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow))
                .ForMember(dest => dest.Assets, act => act.Ignore())
                .ReverseMap();
            CreateMap<NGitLab.Models.ProjectIssueNote, Model.IssueComment>()
                .ForMember(dest => dest.Id, act => act.MapFrom(src => src.NoteId))
                .ReverseMap();
            CreateMap<NGitLab.Models.MergeRequestComment, Model.IssueComment>()
                .ReverseMap();
        }
    }
}
