using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Abstractions.Db.Repositories;
using Annium.Xs.Server.Abstractions.Domain;
using Annium.Xs.Server.Abstractions.Internal.Db;
using Annium.Xs.Server.Abstractions.Internal.Db.Repositories;
using Annium.Xs.Server.Abstractions.Internal.Services;
using Annium.Xs.Server.Abstractions.Internal.Tools;
using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Abstractions.Tools;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Abstractions;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddTools<
        TPackage,
        TPackageDependency,
        TPackageRequest,
        TPackageRequestParser,
        TPackageStorage
    >(this IServiceContainer container, ProjectType projectType)
        where TPackage : class, IPackage<TPackageDependency>
        where TPackageDependency : class, IPackageDependency
        where TPackageRequest : class, IPackageRequest
        where TPackageRequestParser : IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest>
        where TPackageStorage : IPackageStorage<TPackage, TPackageDependency>
    {
        // db
        container.AddPostgreSql<ServerConnection<TPackage, TPackageDependency>>();
        container
            .Add<IPackageRepository<TPackage, TPackageDependency>, PackageRepository<TPackage, TPackageDependency>>()
            .Scoped();

        // services
        container
            .Add<
                IPackageService<TPackage, TPackageDependency, TPackageRequest>,
                PackageService<TPackage, TPackageDependency, TPackageRequest>
            >()
            .Scoped();
        container
            .Add<IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest>, TPackageRequestParser>()
            .Singleton();
        container.Add<TPackageStorage>().AsInterfaces().Singleton();

        // tools
        container
            .Add<UrlTool>(
                static (sp, projectType) =>
                {
                    var config = sp.Resolve<Shared.Configuration>();
                    var uri = config.Servers[projectType.CastTo<ProjectType>()];
                    return new UrlTool(uri);
                }
            )
            .AsKeyed<IUrlTool>(projectType)
            .Singleton();

        return container;
    }
}
