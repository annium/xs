using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.RegistryClient.Shared;

namespace Xs.RegistryClient.Dotnet
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<IProjectClientFactory, DotnetClientFactory>();
            services.AddSingleton<DotnetClient>();
        }
    }
}