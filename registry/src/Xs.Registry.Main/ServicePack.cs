using System;
using System.Collections.Generic;
using Annium.Extensions.DependencyInjection;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Main.Auth;
using Xs.Registry.Main.Tools;

namespace Xs.Registry.Main
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Db.Shared.ServicePack>();
            Add<Shared.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // auth
            services.AddSingleton<AuthorizationFilter>();
            services.AddSingleton<ISessionManager, SessionManager>();

            // tools
            services.AddSingleton<IRegistryManager, RegistryManager>();
            services.AddSingleton<ISecurityManager, SecurityManager>();

            // mapping
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                foreach (var profile in provider.GetRequiredService<IEnumerable<MapperConfigurationExpression>>())
                    cfg.AddProfile(profile);
            });
            mapperConfiguration.AssertConfigurationIsValid();
            services.AddSingleton<IMapper>(mapperConfiguration.CreateMapper());
        }
    }
}