using System;
using Annium.Core.DependencyInjection;
using Server.Abstractions.Packages;
using Server.Dotnet.Payloads;
using Server.Dotnet.Storage;
using Server.Shared.Auth;
using IPackageStorage = Server.Abstractions.Packages.IPackageStorage;

namespace Server.Dotnet;

public class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.Add(new Configuration()).AsSelf().Singleton();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // TODO: setup with index

        // auth
        container.Add<ITokenAccessor>(new HeaderTokenAccessor("X-NuGet-ApiKey")).AsInterfaces().Singleton();
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsInterfaces().Singleton();

        // packages
        container.Add<IPackageService<Package, PackageDependency, PackagePayload>, PackageService<Package, PackageDependency, PackagePayload>>().Scoped();
        container.Add<IPayloadParser<PackagePayload, Package, PackageDependency>, PayloadParser>().Singleton();
        container.Add(Constants.ProjectType).AsSelf().Singleton();

        // storage
        container.Add<IPackageStorage, PackageStorage>().Singleton();
        container.Add<Storage.IPackageStorage, PackageStorage>().Singleton();
        container.Add<ISymbolStorage, SymbolStorage>().Singleton();
    }
}