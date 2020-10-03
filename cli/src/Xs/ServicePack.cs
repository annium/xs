using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Tools;

namespace Xs
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddRuntimeTools(GetType().Assembly, false);
            services.AddConfigurationBuilder();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddMapper();
            services.AddArguments();

            // commands
            services.AddAssemblyTypes(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .AsSelf()
                .SingleInstance();

            // tools
            services.AddSingleton<IConfigurationManager, ConfigurationManager>();
            services.AddSingleton<ProjectsRunner>();
            services.AddSingleton<Watcher>();
        }
    }
}