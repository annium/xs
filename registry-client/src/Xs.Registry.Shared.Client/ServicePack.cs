using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Shared.Client
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<SharedClientFactory>();
            services.AddSingleton<PermissionsClient>();
            services.AddSingleton<SharedClient>();
            services.AddSingleton<UserClient>();
        }
    }
}