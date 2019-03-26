using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Node.Projects;
using Xs.Cli.Node.Tools;

namespace Xs.Cli.Node
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<ISpecialProjectFactory, ProjectFactory>();
            services.AddSingleton<ProjectMapper>();
            services.AddSingleton<ISpecialConfigurationManager, SpecialConfigurationManager>();

            // audit rules
            services.AddAuditRule<FindInconsistentDependenciesRule<ISpecialProject>, ISpecialProject>();
            services.AddAuditRule<FindUselessDependenciesRule<ISpecialProject>, ISpecialProject>();

            RegisterCommands(services);
        }

        private void RegisterCommands(IServiceCollection services)
        {
            // new
            services.AddSingleton<Commands.New.Group>();
            services.AddSingleton<Commands.New.AppReactCommand>();
            services.AddSingleton<Commands.New.LibCommand>();
            services.AddSingleton<Commands.New.LibReactCommand>();
        }
    }
}