using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;
using Xs.Cli.Dotnet.Tools;

namespace Xs.Cli.Dotnet
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<ISpecialProjectFactory, ProjectFactory>();
            services.AddSingleton<ProjectMapper>();
            services.AddSingleton<ISpecialConfigurationManager, SpecialConfigurationManager>();

            // audit rules
            services.AddSingleton<IAuditRule<ISpecialProject>, FindUselessDependenciesRule<ISpecialProject>>();

            RegisterCommands(services);
        }

        private void RegisterCommands(IServiceCollection services)
        {
            // new
            services.AddSingleton<Commands.New.Group>();
            services.AddSingleton<Commands.New.ExeCommand>();
            services.AddSingleton<Commands.New.LibCommand>();
            services.AddSingleton<Commands.New.TestsCommand>();
        }
    }
}