using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Node.Client
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddTransient<INodeClient, NodeClient>();
            services.AddSingleton<INodeClientFactory, NodeClientFactory>();
        }
    }
}