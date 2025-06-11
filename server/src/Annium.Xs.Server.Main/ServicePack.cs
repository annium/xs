using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.DependencyInjection.Packs;
using Annium.Xs.Server.Main.Internal.Services;
using Annium.Xs.Server.Main.Services;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;

namespace Annium.Xs.Server.Main;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // auth
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsInterfaces().Singleton();

        // services
        container.Add<IMetaPackageService, MetaPackageService>().Singleton();
        container.Add<IUserService, UserService>().Singleton();

        // tools
        container.Add<ISecurityService, SecurityService>().Singleton();
    }
}
