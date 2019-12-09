using AutoMapper;

namespace GitReleaseManager.Core
{
    public static class AutoMapperConfiguration
    {
        public static IMapper Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(new[] { typeof(AutoMapperConfiguration) });
            });

            return config.CreateMapper();
        }
    }
}