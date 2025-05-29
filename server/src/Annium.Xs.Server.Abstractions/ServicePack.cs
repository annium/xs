using System;
using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Abstractions.Internal.Services;
using Annium.Xs.Server.Abstractions.Services;

namespace Annium.Xs.Server.Abstractions;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // storage
        container.Add<IStorageFactory, FileStorageFactory>().Singleton();
    }
}
