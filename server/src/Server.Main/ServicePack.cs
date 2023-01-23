using System;
using Annium.Core.DependencyInjection;
using Server.Main.Internal.Auth;
using Server.Main.Internal.Services;
using Server.Main.Services;
using Server.Main.Tools;
using Server.Shared;
using Server.Shared.Auth;
using Server.Shared.Auth.TokenAccessors;

namespace Server.Main;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // auth
        container.AddRegistryAuthorization<AuthorizationFilter>();
        container.Add<Func<Access, AuthorizationFilter>>(sp => access => new AuthorizationFilter(sp, access)).AsSelf().Singleton();
        container.Add<IUserSessionService, UserUserSessionService>().Scoped();
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsInterfaces().Singleton();

        // services
        container.Add<IMetaPackageService, MetaPackageService>().Singleton();
        container.Add<IUserService, UserService>().Singleton();

        // tools
        container.Add<ISecurityService, SecurityService>().Singleton();
    }
}