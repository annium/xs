using Annium.Core.DependencyInjection;
using Server.Abstractions.Domain;
using Server.Abstractions.Internal.Services;
using Server.Abstractions.Services;
using Server.Domain.Interfaces;

namespace Server.Abstractions;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddPackageTools<TPackage, TPackageDependency, TPackageRequest, TPackageRequestParser, TPackageStorage>(this IServiceContainer container)
        where TPackage : class, IPackage<TPackageDependency>
        where TPackageDependency : class, IPackageDependency
        where TPackageRequest : class, IPackageRequest
        where TPackageRequestParser : IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest>
        where TPackageStorage : IPackageStorage<TPackage, TPackageDependency>
    {
        container.Add<IPackageService<TPackage, TPackageDependency, TPackageRequest>, PackageService<TPackage, TPackageDependency, TPackageRequest>>().Scoped();
        container.Add<IPackageRequestParser<TPackage, TPackageDependency, TPackageRequest>, TPackageRequestParser>().Singleton();
        container.Add<TPackageStorage>().AsInterfaces().Singleton();

        return container;
    }
}