using System;
using Annium.Core.DependencyInjection;
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
            // projects
            services.AddSingleton<ISpecialProjectFactory, ProjectFactory>();
            services.AddSingleton<ISpecialProjectLinker, ProjectLinker>();
            services.AddSingleton<ProjectMapper>();
            services.AddSingleton<IDependencyManager, DependencyManager>();

            // tools
            services.AddSingleton<ISpecialConfigurationManager, SpecialConfigurationManager>();

            // audit rules
            services.AddAuditRule<FindInconsistentDependenciesRule<ISpecialProject>, ISpecialProject>();
            services.AddAuditRule<FindUselessDependenciesRule<ISpecialProject>, ISpecialProject>();

            services.AddAssemblyTypes(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .AsSelf()
                .SingleInstance();
        }
    }
}