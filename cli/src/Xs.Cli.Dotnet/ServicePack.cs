using System;
using Annium.Core.DependencyInjection;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Projects;
using Xs.Cli.Dotnet.Tools;

namespace Xs.Cli.Dotnet
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            // projects
            container.Add<ISpecialProjectFactory, ProjectFactory>().Singleton();
            container.Add<ISpecialProjectLinker, ProjectLinker>().Singleton();
            container.Add<ProjectMapper>().AsSelf().Singleton();
            container.Add<IDependencyManager, DependencyManager>().Singleton();

            // tools
            container.Add<ISpecialConfigurationManager, SpecialConfigurationManager>().Singleton();

            // audit rules
            container.AddAuditRule<FindInconsistentDependenciesRule<ISpecialProject>, ISpecialProject>();
            container.AddAuditRule<FindUselessDependenciesRule<ISpecialProject>, ISpecialProject>();

            // commands
            container.AddAll(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .AsSelf()
                .Singleton();
        }
    }
}