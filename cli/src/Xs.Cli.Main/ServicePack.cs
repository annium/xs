using System;
using Annium.Extensions.DependencyInjection;
using Annium.Extensions.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Xs.Cli.Main.Commands;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddArguments();

            RegisterCommands(services);

            RegisterTasks(services);

            // tools
            services.AddSingleton<IConfigurationManager, ConfigurationManager>();
            services.AddSingleton<ProjectsRunner>();
            services.AddSingleton<Watcher>();

            Mapper.AddConfiguration(ConfigureMapping());
        }

        private void RegisterCommands(IServiceCollection services)
        {
            services.AddSingleton<Group>();

            // audit
            services.AddSingleton<Commands.Audit.Group>();
            services.AddSingleton<Commands.Audit.AuditCommand>();
            services.AddSingleton<Commands.Audit.AuditRulesCommand>();

            // ls
            services.AddSingleton<Commands.Ls.Group>();
            services.AddSingleton<Commands.Ls.ListCommand>();
            services.AddSingleton<Commands.Ls.ListInsCommand>();
            services.AddSingleton<Commands.Ls.ListOutsCommand>();

            // remote
            services.AddSingleton<Commands.Remote.Group>();
            services.AddSingleton<Commands.Remote.DeleteCommand>();
            services.AddSingleton<Commands.Remote.RestoreCommand>();
            services.AddSingleton<Commands.Remote.SetCommand>();
            services.AddSingleton<Commands.Remote.SetLocalCommand>();
            services.AddSingleton<Commands.Remote.ShowCommand>();

            // new
            services.AddSingleton<Commands.New.Group>();

            // root
            services.AddSingleton<AddCommand>();
            services.AddSingleton<BuildCommand>();
            services.AddSingleton<CleanCommand>();
            services.AddSingleton<DeleteCommand>();
            services.AddSingleton<MoveCommand>();
            services.AddSingleton<InstallCommand>();
            services.AddSingleton<PublishCommand>();
            services.AddSingleton<SearchCommand>();
            services.AddSingleton<TestCommand>();
            services.AddSingleton<UnpublishCommand>();
            services.AddSingleton<UpdateCommand>();
            services.AddSingleton<UseCommand>();
            services.AddSingleton<WatchCommand>();
        }

        private void RegisterTasks(IServiceCollection services)
        {
            // dependencies
            services.AddSingleton<Tasks.Dependencies.AddPackageDependencyTask>();
            services.AddSingleton<Tasks.Dependencies.AddProjectDependencyTask>();
            services.AddSingleton<Tasks.Dependencies.DeletePackageDependencyTask>();
            services.AddSingleton<Tasks.Dependencies.DeleteProjectDependencyTask>();

            // root
            services.AddSingleton<DiscoverProjectsTask>();
        }

        private MapperConfiguration ConfigureMapping()
        {
            var cfg = new MapperConfiguration();

            cfg.Map<string, Core.Models.Version>(s => new Core.Models.Version(s));
            cfg.Map<string, Core.Models.ProjectType>(s => Core.Models.ProjectType.Get(s));

            return cfg;
        }
    }
}