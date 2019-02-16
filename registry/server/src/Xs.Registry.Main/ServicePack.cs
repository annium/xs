using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Main.Auth;
using Xs.Registry.Main.Tools;
using Xs.Registry.Shared.Helpers;

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
            services.AddScoped<ISessionManager, SessionManager>();

            // tools
            services.AddSingleton<IRegistryManager, RegistryManager>();
            services.AddSingleton<ISecurityManager, SecurityManager>();

            // mapping
            services.AddAutoMapper(provider);
        }
    }
}