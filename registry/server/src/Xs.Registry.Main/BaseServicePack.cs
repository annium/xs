using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Main.Auth;
using Xs.Registry.Main.Tools;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Main
{
    internal class BaseServicePack : ServicePackBase
    {
        public BaseServicePack()
        {
            Add<Shared.ServicePack>();
            Add<Db.Shared.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // auth
            services.AddSingleton<Func<Access, AuthorizationFilter>>(sp => access => new AuthorizationFilter(sp, access));
            services.AddScoped<ISessionManager, SessionManager>();
            services.AddSingleton<ITokenAccessor>(new BearerTokenAccessor());

            // tools
            services.AddSingleton<ISecurityManager, SecurityManager>();

            // mapping
            services.AddMapper(provider);
        }
    }
}