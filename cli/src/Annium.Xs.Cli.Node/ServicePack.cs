using System;
using System.Net;
using System.Net.Http;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Annium.Xs.Cli.Core.Audit;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tools;
using Annium.Xs.Cli.Node.Projects;
using Annium.Xs.Cli.Node.Tools;

namespace Annium.Xs.Cli.Node;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // projects
        container.Add<IPlatformProjectFactory, ProjectFactory>().Singleton();
        container.Add<IPlatformProjectLinker, ProjectLinker>().Singleton();
        container.Add<ProjectMapper>().AsSelf().Singleton();
        container.Add<IDependencyManager, DependencyManager>().Singleton();
        container.AddHttpRequestFactory(
            "node",
            (_, _) =>
                new HttpClient(
                    new HttpClientHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip,
                        MaxConnectionsPerServer = 16,
                    }
                )
        );

        // tools
        container.Add<IPlatformConfigurationManager, PlatformConfigurationManager>().Singleton();

        // audit rules
        container.AddAuditRule<FindInconsistentDependenciesRule<IPlatformProject>, IPlatformProject>();
        container.AddAuditRule<FindUselessDependenciesRule<IPlatformProject>, IPlatformProject>();
    }
}
