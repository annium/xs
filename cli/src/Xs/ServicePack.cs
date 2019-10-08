using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Xs.Commands;
using Xs.Tools;

namespace Xs
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddArguments();

            RegisterCommands(services);

            // tools
            services.AddSingleton<IConfigurationManager, ConfigurationManager>();
            services.AddSingleton<ProjectsRunner>();
            services.AddSingleton<Watcher>();

            Mapper.AddConfiguration(ConfigureProfile);
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

            // root
            services.AddSingleton<AddCommand>();
            services.AddSingleton<BuildCommand>();
            services.AddSingleton<CleanCommand>();
            services.AddSingleton<DeleteCommand>();
            services.AddSingleton<FormatCommand>();
            services.AddSingleton<InstallCommand>();
            services.AddSingleton<MoveCommand>();
            services.AddSingleton<PublishCommand>();
            services.AddSingleton<SearchCommand>();
            services.AddSingleton<TestCommand>();
            services.AddSingleton<ToggleCommand>();
            services.AddSingleton<UnpublishCommand>();
            services.AddSingleton<UpdateCommand>();
            services.AddSingleton<UseCommand>();
            services.AddSingleton<WatchCommand>();
        }

        private void ConfigureProfile(Profile p)
        {
            p.Map<string, Cli.Core.Models.Version>(s => Cli.Core.Models.Version.Parse(s));
            p.Map<string, Cli.Core.Models.ProjectType>(s => Cli.Core.Models.ProjectType.Get(s));
        }
    }
}