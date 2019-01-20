using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Core.Client;

namespace Xs.Registry.Node.Client
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<IProjectClientFactory, NodeClientFactory>();
            services.AddSingleton<NodeClient>();

            services.AddSingleton<InfoClient>();
        }
    }
}