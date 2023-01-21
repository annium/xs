using System;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Server.Host.Auth;
using Server.Host.Tools;
using Server.Shared;
using Server.Shared.Auth;

namespace Server.Host;

internal class BaseServicePack : ServicePackBase
{
    public BaseServicePack()
    {
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

        // helpers
        container.Add<IHttpContextAccessor, HttpContextAccessor>().Singleton();
        container.Add<IActionContextAccessor, ActionContextAccessor>().Singleton();
        container.Add<IUrlHelper>(p =>
        {
            var actionContext = p.GetRequiredService<IActionContextAccessor>().ActionContext ??
                throw new InvalidOperationException($"Resolved null {nameof(ActionContext)}");

            return p.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
        }).AsSelf().Scoped();


        // auth
        container.Add<Func<Access, AuthorizationFilter>>(sp => access => new AuthorizationFilter(sp, access)).AsSelf().Singleton();
        container.Add<ISessionManager, SessionManager>().Scoped();
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsInterfaces().Singleton();

        // tools
        container.Add<ISecurityManager, SecurityManager>().Singleton();

        // host
        container.AddRegistryAuthorization<AuthorizationFilter>();
        container.Collection.AddCors();
        container.Collection.AddControllers()
            .AddApplicationPart(typeof(Dotnet.ServicePack).Assembly)
            .AddApplicationPart(typeof(Node.ServicePack).Assembly)
            .AddDefaultJsonOptions();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}