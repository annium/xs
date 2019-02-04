using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Abstract.Tools;

namespace Xs.Registry.Abstract
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Shared.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // tools
            services.AddSingleton<IRegistryConnectionManager, RegistryConnectionManager>();
        }
    }
}