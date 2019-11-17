using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Xs.Tools;

namespace Xs
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddArguments();

            // commands
            services.SelectAssemblyTypes()
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .RegisterSingleton();

            // tools
            services.AddSingleton<IConfigurationManager, ConfigurationManager>();
            services.AddSingleton<ProjectsRunner>();
            services.AddSingleton<Watcher>();

            Mapper.AddConfiguration(ConfigureProfile);
        }

        private void ConfigureProfile(Profile p)
        {
            p.Map<string, Cli.Core.Models.Version>(s => Cli.Core.Models.Version.Parse(s));
            p.Map<string, Cli.Core.Models.ProjectType>(s => Cli.Core.Models.ProjectType.Get(s));
        }
    }
}