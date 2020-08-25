// -----------------------------------------------------------------------
// <copyright file="AutoMapperConfiguration.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using AutoMapper;
    using Serilog;

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