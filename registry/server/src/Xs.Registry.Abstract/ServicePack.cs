using System;
using Annium.Core.DependencyInjection;
using Xs.Registry.Abstract.Auth;
using Xs.Registry.Abstract.Storage;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Abstract;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Db.Shared.ServicePack>();
        Add<Shared.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // auth
        container.Add<Func<Access, AuthorizationFilter>>(sp => access => new AuthorizationFilter(sp)).AsSelf().Singleton();

        // storage
        container.Add<IStorageFactory, FileStorageFactory>().Singleton();
    }
}