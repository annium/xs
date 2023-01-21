using System;
using Annium.Core.DependencyInjection;
using Server.Host.Auth;
using Server.Host.Tools;
using Server.Shared.Auth;

namespace Server.Host;

internal class BaseServicePack : ServicePackBase
{
    public BaseServicePack()
    {
        Add<Shared.ServicePack>();
        Add<Xs.Registry.Db.Shared.ServicePack>();
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