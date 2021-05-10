using System;
using Annium.Core.DependencyInjection;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Dotnet.Payloads;
using Xs.Registry.Dotnet.Storage;
using Xs.Registry.Shared.Auth;
using IPackageStorage = Xs.Registry.Abstract.Packages.IPackageStorage;

namespace Xs.Registry.Dotnet
{
    internal class BaseServicePack : ServicePackBase
    {
        public BaseServicePack()
        {
            Add<Abstract.ServicePack>();
            Add<Db.Dotnet.ServicePack>();
        }

        public override void Configure(IServiceContainer container)
        {
            container.AddRuntimeTools(GetType().Assembly, false);
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
}