using System;
using Annium.Core.DependencyInjection;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Node.Projects;
using Xs.Cli.Node.Tools;

namespace Xs.Cli.Node;

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
