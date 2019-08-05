using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.RegistryClient.Main
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<MainClientFactory>();
            services.AddTransient<MainClient>();
        }
    }
}