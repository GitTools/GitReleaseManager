using AutoMapper;
using Octokit;

namespace GitReleaseManager.Core
{
    public static class AutoMapperConfiguration
    {
        public static IMapper Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Model.Issue, Issue>();
                cfg.CreateMap<Model.Release, Release>();
                cfg.CreateMap<Model.Label, Label>();
                cfg.CreateMap<Model.Milestone, Milestone>();
                cfg.CreateMap<Issue, Model.Issue>();
                cfg.CreateMap<Release, Model.Release>();
                cfg.CreateMap<Label, Model.Label>();
                cfg.CreateMap<Milestone, Model.Milestone>()
                    .AfterMap((src, dest) => dest.Version = src.Version());
            });

            return config.CreateMapper();
        }
    }
}