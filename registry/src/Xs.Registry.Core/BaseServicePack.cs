using System;
using Annium.Extensions.DependencyInjection;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Models;
using Xs.Registry.Core.Security;
using Xs.Registry.Core.Storage;
using Xs.Registry.Core.Tools;

namespace Xs.Registry.Core
{
    public class BaseServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton<MapperConfigurationExpression>(ConfigureMapping());
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(() => SystemClock.Instance.GetCurrentInstant());

            // auth
            services.AddSingleton<IAuthorizationFilterFactory, AuthorizationFilterFactory>();
            services.AddSingleton<ApiAuthorizationFilter>();
            services.AddSingleton<SessionAuthorizationFilter>();
            services.AddSingleton<ISessionManager, SessionManager>();

            // helpers
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(p =>
            {
                var actionContext = p.GetRequiredService<IActionContextAccessor>().ActionContext;
                return p.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
            });

            // security
            services.AddSingleton<ISecurityManager, SecurityManager>();

            // storage
            services.AddSingleton<IStorageFactory, FileStorageFactory>();

            // tools
            // services.AddSingleton<IMetaPackageManager, MetaPackageManager>();
            services.AddSingleton<IRegistryConnectorFactory, RegistryConnectorFactory>();
            services.AddSingleton<IRegistryManager, RegistryManager>();
        }

        private MapperConfigurationExpression ConfigureMapping()
        {
            var cfg = new MapperConfigurationExpression();

            cfg.CreateMap<User, Db.Models.User>().ReverseMap();
            cfg.CreateMap<UserSession, Db.Models.UserSession>().ReverseMap();

            return cfg;
        }
    }
}