using System;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Server.Host;

internal class BaseServicePack : ServicePackBase
{
    public BaseServicePack()
    {
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

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}