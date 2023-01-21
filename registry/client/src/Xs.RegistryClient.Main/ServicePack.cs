using System;
using Annium.Core.DependencyInjection;
using Xs.RegistryClient.Main.Clients;
using Xs.RegistryClient.Server;
using Xs.RegistryClient.Server.Clients;

namespace Xs.RegistryClient.Main;

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