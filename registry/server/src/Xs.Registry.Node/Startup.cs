using System;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Xs.Registry.Abstract.Auth;
using Xs.Registry.Abstract.Tools;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Node
{
    public class Startup<TServicePack> where TServicePack : ServicePackBase, new()
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddRegistryAuthorization<AuthorizationFilter>();

            services.AddMvc()
                .AddJsonOptions(opts => opts.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Serialization));

            return new ServiceProviderBuilder(services)
                .UseServicePack<TServicePack>()
                .Build();
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime)
        {
            // initialize registry connection
            app.ApplicationServices
                .GetRequiredService<IRegistryConnectionManager>()
                .Setup(Constants.ProjectType, app.ApplicationServices.GetRequiredService<Configuration>(), lifetime);

            app.UseExceptionMiddleware();

            app.UseCors(builder => builder
                .SetIsOriginAllowed(o => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseMvc();
        }
    }
}