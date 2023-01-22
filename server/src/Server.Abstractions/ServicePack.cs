using System;
using Annium.Core.DependencyInjection;
using Server.Abstractions.Internal.Auth;
using Server.Abstractions.Internal.Services;
using Server.Abstractions.Services;
using Server.Shared.Auth;

namespace Server.Abstractions;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // auth
        container.Add<Func<Access, AuthorizationFilter>>(sp => _ => new AuthorizationFilter(sp)).AsSelf().Singleton();

        // storage
        container.Add<IStorageFactory, FileStorageFactory>().Singleton();
    }
}