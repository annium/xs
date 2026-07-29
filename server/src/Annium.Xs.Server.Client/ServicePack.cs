using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Client.Clients;

namespace Annium.Xs.Server.Client;

public class ServicePack : ServicePackBase
{
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.Add<MainClientFactory>().AsSelf().Singleton();
        container.Add<MainClient>().AsSelf().Transient();

        container.Add<ServerClientFactory>().AsSelf().Singleton();
        container.Add<ServerClient>().AsSelf().Transient();

        return Task.CompletedTask;
    }
}
