using System;
using Annium.Core.DependencyInjection;
using Server.Abstractions.Packages;
using Server.Dotnet.Payloads;
using Server.Dotnet.Storage;
using Server.Shared.Auth;
using Xs.Registry.Db.Dotnet.Models;
using IPackageStorage = Server.Abstractions.Packages.IPackageStorage;

namespace Server.Dotnet;

internal class BaseServicePack : ServicePackBase
{
    public BaseServicePack()
    {
        Add<Abstractions.ServicePack>();
        Add<Xs.Registry.Db.Dotnet.ServicePack>();
    }

    public override void Configure(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
        container.Add(new Configuration()).AsSelf().Singleton();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
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

        // mapping
        container.AddMapper();
    }
}