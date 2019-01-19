using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;

namespace Xs.Registry.Shared
{
    public class Startup<TServicePack> where TServicePack : ServicePackBase, new()
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddRegistryAuthorization();

            services.AddMvc();

            return new ServiceProviderBuilder(services)
                .UseServicePack<TServicePack>()
                .Build();
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime)
        {
            app.UseExceptionMiddleware();

            app.UseMvc();
        }
    }
}