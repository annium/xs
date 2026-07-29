using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Core.Runtime;
using Annium.Extensions.Arguments;
using Annium.Xs.Cli.Commands.Sync;
using Annium.Xs.Cli.Tools;

namespace Annium.Xs.Cli;

public class ServicePack : ServicePackBase
{
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.AddRuntime(GetType().Assembly);

        return Task.CompletedTask;
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddMapper();
        container.AddArguments();

        // tasks
        container.AddAll().Where(x => x.Name.EndsWith("Task")).AsSelf().Singleton();

        // tools
        container.Add<SyncConfigurator>().AsSelf().Singleton();
        container.Add<ProjectsRunner>().AsSelf().Singleton();
        container.Add<Watcher>().AsSelf().Singleton();
        container.Add<WebServer>().AsSelf().Singleton();

        return Task.CompletedTask;
    }
}
