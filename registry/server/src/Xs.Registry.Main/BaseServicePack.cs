using System;
using Annium.Core.DependencyInjection;
using Xs.Registry.Main.Auth;
using Xs.Registry.Main.Tools;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Main;

internal class BaseServicePack : ServicePackBase
{
    public BaseServicePack()
    {
        Add<Shared.ServicePack>();
        Add<Db.Shared.ServicePack>();
    }

    public override void Configure(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // auth
        container.Add<Func<Access, AuthorizationFilter>>(sp => access => new AuthorizationFilter(sp, access)).AsSelf().Singleton();
        container.Add<ISessionManager, SessionManager>().Scoped();
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsInterfaces().Singleton();

        // tools
        container.Add<ISecurityManager, SecurityManager>().Singleton();

        // mapping
        container.AddMapper();
    }
}