using System;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Swashbuckle.AspNetCore.Swagger;
using Xs.Registry.Abstract.Auth;
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
                .AddJsonOptions(opts => opts.SerializerSettings
                    .ConfigureForNodaTime(DateTimeZoneProviders.Serialization)
                    .ConfigureForOperations()
                );

            services.AddSwaggerGen(o =>
            {
                var info = new Info();
                info.Title = "Registry Node";
                info.Version = "v1";
                o.SwaggerDoc("v1", info);
            });

            return new ServiceProviderBuilder(services)
                .UseServicePack<TServicePack>()
                .Build();
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime lifetime)
        {
            app.UseExceptionMiddleware();

            app.UseCors(builder => builder
                .SetIsOriginAllowed(o => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseSwagger(o =>
            {
                o.RouteTemplate = "docs/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(o =>
            {
                o.RoutePrefix = "docs";
                o.SwaggerEndpoint("v1/swagger.json", "Registry Node Api v1");
            });

            app.UseMvc();
        }
    }
}