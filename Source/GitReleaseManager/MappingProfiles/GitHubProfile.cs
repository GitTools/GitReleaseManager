using AutoMapper;

namespace GitReleaseManager.Core.MappingProfiles

{
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
