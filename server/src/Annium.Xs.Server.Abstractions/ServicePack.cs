using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Abstractions.Internal.Services;
using Annium.Xs.Server.Abstractions.Services;

namespace Annium.Xs.Server.Abstractions;

public class ServicePack : ServicePackBase
{
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        // storage
        container.Add<IStorageFactory, FileStorageFactory>().Singleton();

        return Task.CompletedTask;
    }
}
