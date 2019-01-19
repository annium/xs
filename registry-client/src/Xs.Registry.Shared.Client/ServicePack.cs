using System;
using System.Net.Http;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Shared.Client
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<SharedClient>();
            services.AddSingleton<SharedClientFactory>();
            services.AddTransient<HttpClient>();
        }
    }
}