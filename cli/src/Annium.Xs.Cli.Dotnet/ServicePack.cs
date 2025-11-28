using System;
using System.Net;
using System.Net.Http;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Annium.Xs.Cli.Core.Audit;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tools;
using Annium.Xs.Cli.Dotnet.Projects;
using Annium.Xs.Cli.Dotnet.Tools;

namespace Annium.Xs.Cli.Dotnet;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // projects
        container.Add<IPlatformProjectFactory, ProjectFactory>().Singleton();
        container.Add<IPlatformProjectLinker, ProjectLinker>().Singleton();
        container.Add<ProjectMapper>().AsSelf().Singleton();
        container.Add<IDependencyManager, DependencyManager>().Singleton();
        container.AddHttpRequestFactory("dotnet");
        container.AddHttpRequestFactory(
            "dotnet",
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
        container.Add<IPropsFilesManager, PropsFilesManager>().Singleton();

        // audit rules
        container.AddAuditRule<FindInconsistentDependenciesRule<IPlatformProject>, IPlatformProject>();
        container.AddAuditRule<FindUselessDependenciesRule<IPlatformProject>, IPlatformProject>();
    }
}
