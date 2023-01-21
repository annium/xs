using System;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Server.Shared;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRealTime().SetDefault();

        // helpers
        container.Add<IHttpContextAccessor, HttpContextAccessor>().Singleton();
        container.Add<IActionContextAccessor, ActionContextAccessor>().Singleton();
        container.Add<IUrlHelper>(p =>
        {
            var actionContext = p.GetRequiredService<IActionContextAccessor>().ActionContext;
            return p.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
        }).AsSelf().Scoped();

        container.AddSerializers().WithJson(isDefault: true);
        container.AddHttpRequestFactory().SetDefault();
        container.AddMediator();

        container.AddLogging();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}