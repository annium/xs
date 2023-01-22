using Annium.Core.DependencyInjection;
using Server.Abstractions.Domain;
using Server.Abstractions.Internal.Services;
using Server.Abstractions.Services;
using Server.Domain.Interfaces;

namespace Server.Abstractions;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddPackageTools<TPackage, TPackageDependency, TPackagePayload, TPayloadParser, TPackageStorage>(this IServiceContainer container)
        where TPackage : class, IPackage<TPackageDependency>
        where TPackageDependency : class, IPackageDependency
        where TPackagePayload : class, IPayload
        where TPayloadParser : IPayloadParser<TPackage, TPackageDependency, TPackagePayload>
        where TPackageStorage : IPackageStorage<TPackage, TPackageDependency>
    {
        container.Add<IPackageService<TPackage, TPackageDependency, TPackagePayload>, PackageService<TPackage, TPackageDependency, TPackagePayload>>().Scoped();
        container.Add<IPayloadParser<TPackage, TPackageDependency, TPackagePayload>, TPayloadParser>().Singleton();
        container.Add<TPackageStorage>().AsInterfaces().Singleton();

        return container;
    }
}