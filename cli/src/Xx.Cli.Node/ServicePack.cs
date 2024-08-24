using System;
using Annium.Core.DependencyInjection;
using Xx.Cli.Core.Audit;
using Xx.Cli.Core.Projects;
using Xx.Cli.Core.Tools;
using Xx.Cli.Node.Projects;
using Xx.Cli.Node.Tools;

namespace Xx.Cli.Node;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // projects
        container.Add<IPlatformProjectFactory, ProjectFactory>().Singleton();
        container.Add<IPlatformProjectLinker, ProjectLinker>().Singleton();
        container.Add<ProjectMapper>().AsSelf().Singleton();
        container.Add<IDependencyManager, DependencyManager>().Singleton();

        // tools
        container.Add<IPlatformConfigurationManager, PlatformConfigurationManager>().Singleton();

        // audit rules
        container.AddAuditRule<FindInconsistentDependenciesRule<IPlatformProject>, IPlatformProject>();
        container.AddAuditRule<FindUselessDependenciesRule<IPlatformProject>, IPlatformProject>();
    }
}
