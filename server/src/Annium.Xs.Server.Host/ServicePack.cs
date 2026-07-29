using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.AspNetCore.Extensions;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Yaml;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.linq2db.PostgreSql;
using Annium.Logging.Console;
using Annium.Logging.Shared;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Annium.Xs.Server.Host;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Shared.ServicePack>();
        Add<Abstractions.ServicePack>();
        Add<Main.ServicePack>();
        Add<Dotnet.ServicePack>();
        Add<Node.ServicePack>();
    }

    public override async Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddConfiguration(new WebHostConfiguration());
        await container.AddConfigurationAsync<Shared.Configuration>(
            x =>
            {
                x.AddYamlFile(Path.Combine("configuration", "main.yml"));
            },
            ct
        );
        await container.AddConfigurationAsync<PostgreSqlConfiguration>(
            x =>
            {
                x.AddYamlFile(Path.Combine("configuration", "db.yml"));
            },
            ct
        );
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddTime().WithRealTime().SetDefault();
        container.AddHttpRequestFactory(true);
        container.AddSerializers().WithJson(isDefault: true);
        container.AddMapper();
        container.AddLogging();

        // host
        container.Collection.AddCors();
        container
            .Collection.AddControllers()
            .AddApplicationPart(typeof(Main.ServicePack).Assembly)
            .AddApplicationPart(typeof(Dotnet.ServicePack).Assembly)
            .AddApplicationPart(typeof(Node.ServicePack).Assembly)
            .AddDefaultJsonOptions();
        container.Collection.AddSwaggerGen(SetupSwagger);

        // host helpers
        container.Add<IHttpContextAccessor, HttpContextAccessor>().Singleton();

        return Task.CompletedTask;
    }

    private void SetupSwagger(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Package server", Version = "v1" });
        options.AddSecurityDefinition(
            "token",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer",
            }
        );
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("token", document)] = [],
        });
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseConsole());

        return Task.CompletedTask;
    }
}
