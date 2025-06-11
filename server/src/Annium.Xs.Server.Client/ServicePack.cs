using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.DependencyInjection.Packs;
using Annium.Xs.Server.Client.Clients;

namespace Annium.Xs.Server.Client;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<MainClientFactory>().AsSelf().Singleton();
        container.Add<MainClient>().AsSelf().Transient();

        container.Add<ServerClientFactory>().AsSelf().Singleton();
        container.Add<ServerClient>().AsSelf().Transient();
    }
}
