using System;
using Annium.Core.DependencyInjection;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Node.Models;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Node.Storage;
using Xs.Registry.Shared.Auth;
using IPackageStorage = Xs.Registry.Abstract.Packages.IPackageStorage;

namespace Xs.Registry.Node;

internal class BaseServicePack : ServicePackBase
{
    public BaseServicePack()
    {
        Add<Abstract.ServicePack>();
        Add<Db.Node.ServicePack>();
    }

    public override void Configure(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
        container.Add(new Configuration()).AsSelf().Singleton();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // auth
        container.Add<ITokenAccessor>(new BearerTokenAccessor()).AsSelf().Singleton();

        // packages
        container.Add<IPackageService<Package, PackageDependency, PackagePayload>, PackageService<Package, PackageDependency, PackagePayload>>().Scoped();
        container.Add<IPayloadParser<PackagePayload, Package, PackageDependency>, PayloadParser>().Singleton();
        container.Add(Constants.ProjectType).AsSelf().Singleton();

        // storage
        container.Add<IPackageStorage, PackageStorage>().Singleton();
        container.Add<Storage.IPackageStorage, PackageStorage>().Singleton();

        // mapping
        container.AddMapper();
    }
}