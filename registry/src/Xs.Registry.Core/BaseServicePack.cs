using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Security;
using Xs.Registry.Core.Storage;
using Xs.Registry.Core.Tools;

namespace Xs.Registry.Core
{
    public class BaseServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<DateTime>>(() => DateTime.UtcNow);

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
            services.AddSingleton<IMetadataManager, MetadataManager>();
            services.AddSingleton<IRegistryConnectorFactory, RegistryConnectorFactory>();
            services.AddSingleton<IRegistryManager, RegistryManager>();
        }
    }
}