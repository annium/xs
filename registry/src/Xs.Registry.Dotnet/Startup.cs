using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Core.Auth;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Tools;

namespace Xs.Registry.Dotnet
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
            // initialize registry connection
            var cfg = GetService<Configuration>();
            var registryConnector = GetService<IRegistryConnectorFactory>().Create(cfg.SharedLocation, Constants.ProjectType, cfg.Location);
            lifetime.ApplicationStarted.Register(registryConnector.Connect);
            lifetime.ApplicationStopping.Register(registryConnector.Disconnect);

            app.UseExceptionMiddleware();

            app.UseMvc();

            T GetService<T>() => app.ApplicationServices.GetRequiredService<T>();
        }
    }
}