using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.RegistryClient.Server
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<ServerClientFactory>();
            services.AddTransient<ServerClient>();
        }
    }
}