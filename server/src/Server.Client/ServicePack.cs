using System;
using Annium.Core.DependencyInjection;
using Server.Client.Clients;

namespace Server.Client;

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
