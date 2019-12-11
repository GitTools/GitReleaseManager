// -----------------------------------------------------------------------
// <copyright file="AutoMapperConfiguration.cs" company="GitTools Contributors">
// Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
// -----------------------------------------------------------------------

namespace GitReleaseManager.Core
{
    using AutoMapper;

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