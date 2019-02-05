using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Shared.Helpers
{
    public static class AutoMapperExtensions
    {
        public static void AddAutoMapper(this IServiceCollection services, IServiceProvider provider)
        {
            var cfg = new MapperConfigurationExpression();
            foreach (var profile in provider.GetRequiredService<IEnumerable<MapperConfigurationExpression>>())
                cfg.AddProfile(profile);

            var mapperConfiguration = new MapperConfiguration(cfg);

            mapperConfiguration.AssertConfigurationIsValid();

            services.AddSingleton<IMapper>(mapperConfiguration.CreateMapper());
        }
    }
}