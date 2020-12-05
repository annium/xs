using System;
using Annium.Core.DependencyInjection;
using Xs.Tools;

namespace Xs
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceContainer container)
        {
            container.AddRuntimeTools(GetType().Assembly, false);
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddMapper();
            container.AddArguments();

            // commands
            container.AddAll(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .AsSelf()
                .Singleton();

            // tools
            container.Add<IConfigurationManager, ConfigurationManager>().Singleton();
            container.Add<ProjectsRunner>().AsSelf().Singleton();
            container.Add<Watcher>().AsSelf().Singleton();
            container.Add<WebServerFactory>().AsSelf().Singleton();
        }
    }
}