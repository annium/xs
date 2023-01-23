using System;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Server.Host;

internal class BaseServicePack : ServicePackBase
{
    public BaseServicePack()
    {
        Add<Abstractions.ServicePack>();
        Add<Db.ServicePack>();
        Add<Main.ServicePack>();
        Add<Dotnet.ServicePack>();
        Add<Node.ServicePack>();
    }

    public override void Configure(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRealTime().SetDefault();
        container.AddHttpRequestFactory().SetDefault();
        container.AddSerializers().WithJson(isDefault: true);
        container.AddMapper();
        container.AddLogging();

        // host
        container.Collection.AddCors();
        container.Collection.AddControllers()
            .AddApplicationPart(typeof(Main.ServicePack).Assembly)
            .AddApplicationPart(typeof(Dotnet.ServicePack).Assembly)
            .AddApplicationPart(typeof(Node.ServicePack).Assembly)
            .AddDefaultJsonOptions();
        container.Collection.AddSwaggerGen(SetupSwagger);

        // host helpers
        container.Add<IHttpContextAccessor, HttpContextAccessor>().Singleton();
        container.Add<IActionContextAccessor, ActionContextAccessor>().Singleton();
        container.Add<IUrlHelper>(p =>
        {
            var actionContext = p.GetRequiredService<IActionContextAccessor>().ActionContext ??
                throw new InvalidOperationException($"Resolved null {nameof(ActionContext)}");

            return p.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
        }).AsSelf().Scoped();
    }

    private void SetupSwagger(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Package server", Version = "v1", });
        options.AddSecurityDefinition("token",
            new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}