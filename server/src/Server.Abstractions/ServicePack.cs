using System;
using Annium.Core.DependencyInjection;
using Server.Abstractions.Internal.Services;
using Server.Abstractions.Services;

namespace Server.Abstractions;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // storage
        container.Add<IStorageFactory, FileStorageFactory>().Singleton();
    }
}
