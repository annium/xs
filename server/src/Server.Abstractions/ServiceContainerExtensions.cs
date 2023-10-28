using Annium.Core.DependencyInjection;
using Server.Abstractions.Db.Repositories;
using Server.Abstractions.Domain;
using Server.Abstractions.Internal.Db;
using Server.Abstractions.Internal.Db.Repositories;
using Server.Abstractions.Internal.Services;
using Server.Abstractions.Internal.Tools;
using Server.Abstractions.Services;
using Server.Abstractions.Tools;
using Server.Shared;
using Server.Shared.Domain.Interfaces;
using Server.Shared.Domain.Models;

namespace Server.Abstractions;

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
            .Add<UrlTool>(sp => new UrlTool(sp.Resolve<Configuration>().Servers[projectType]))
            .AsKeyed<IUrlTool, ProjectType>(projectType)
            .Singleton();

        return container;
    }
}
