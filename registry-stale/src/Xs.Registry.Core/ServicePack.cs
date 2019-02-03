using System;
using System.Collections.Generic;
using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xs.Registry.Core.Db;
using Z.EntityFramework.Plus;

namespace Xs.Registry.Core
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<BaseServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(
                new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("configuration", "core.json"))
                .AddJsonFile(Path.Combine("configuration", "core.override.json"), optional : true)
                .Build<Configuration>()
            );
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // db
            RegisterDb(services, provider);

            // repositories
            // services.AddSingleton<IMetaPackageRepository, MetaPackageRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IUserSessionRepository, UserSessionRepository>();

            // mapping
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                foreach (var profile in provider.GetRequiredService<IEnumerable<MapperConfigurationExpression>>())
                    cfg.AddProfile(profile);
            });
            mapperConfiguration.AssertConfigurationIsValid();
            services.Replace(new ServiceDescriptor(typeof(IMapper), mapperConfiguration.CreateMapper()));
        }

        private void RegisterDb(IServiceCollection services, IServiceProvider provider)
        {
            var cfg = provider.GetRequiredService<Configuration>().Database;
            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<CoreDbContext>(builder =>
                {
                    builder.UseNpgsql(string.Join(';', new string[]
                    {
                        $"Host={cfg.Host}",
                        $"Port={cfg.Port}",
                        $"Database={cfg.Name}",
                        $"Username={cfg.User}",
                        $"Password={cfg.Password}",
                    }), options => options.UseNodaTime());
                    builder.EnableSensitiveDataLogging();
                });
            if (cfg.LogQueries)
            {
                Action<System.Data.Common.DbCommand> executing = command => Console.WriteLine(
                    $"{command.CommandType} batch command executing: {command.CommandText}"
                );
                BatchDeleteManager.BatchDeleteBuilder = builder => builder.Executing = executing;
                BatchUpdateManager.BatchUpdateBuilder = builder => builder.Executing = executing;
            }
        }
    }
}