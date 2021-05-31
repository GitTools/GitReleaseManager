using AutoMapper;
using Serilog;

namespace GitReleaseManager.Core
{
    public static class AutoMapperConfiguration
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(AutoMapperConfiguration));

        public static IMapper Configure()
        {
            _logger.Debug("Creating mapper configuration");
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(new[] { typeof(AutoMapperConfiguration) });
            });

            var mapper = config.CreateMapper();

            _logger.Debug("Finished creating mapper configuration");

            return mapper;
        }
    }
}