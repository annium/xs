using System;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Shared
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();

            // helpers
            container.Add<IHttpContextAccessor, HttpContextAccessor>().Singleton();
            container.Add<IActionContextAccessor, ActionContextAccessor>().Singleton();
            container.Add<IUrlHelper>(p =>
            {
                var actionContext = p.GetRequiredService<IActionContextAccessor>().ActionContext;
                return p.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
            }).AsSelf().Scoped();

            container.AddJsonSerializers().SetDefault();
            container.AddHttpRequestFactory().SetDefault();
            container.AddMediator();

            container.AddLogging(route => route.UseConsole());
        }
    }
}