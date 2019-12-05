using AutoMapper;

namespace GitReleaseManager.Core
{
    public static class AutoMapperConfiguration
    {
        public static IMapper Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Model.Issue, Octokit.Issue>();
                cfg.CreateMap<Model.Release, Octokit.Release>();
                cfg.CreateMap<Model.Label, Octokit.Label>();
                cfg.CreateMap<Model.Milestone, Octokit.Milestone>();
                cfg.CreateMap<Octokit.Issue, Model.Issue>();
                cfg.CreateMap<Octokit.Release, Model.Release>();
                cfg.CreateMap<Octokit.Label, Model.Label>();
                cfg.CreateMap<Octokit.Milestone, Model.Milestone>()
                    .AfterMap((src, dest) => dest.Version = src.Version());
            });

            return config.CreateMapper();
        }
    }
}