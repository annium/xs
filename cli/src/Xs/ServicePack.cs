using System;
using Annium.Core.DependencyInjection;
using Xs.Commands.Sync;
using Xs.Tools;

namespace Xs;

public class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
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
    }
}
