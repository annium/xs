using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Host.Auth;
using Server.Shared.Auth;

namespace Server.Host;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddCors();
        services.AddRegistryAuthorization<AuthorizationFilter>();
        services.AddMvc()
            .AddDefaultJsonOptions();
        services.AddOpenApiDocument();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseExceptionMiddleware();
        if (env.IsDevelopment())
        {
            app.UseStaticFiles();
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }

        app.UseRouting();
        app.UseCors(builder => builder
            .SetIsOriginAllowed(o => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}