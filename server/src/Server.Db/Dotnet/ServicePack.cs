using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using LinqToDB;
using Server.Db.Dotnet.Entities;
using Server.Db.Shared.Repositories;

namespace Server.Db.Dotnet;

public class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddProfile(ConfigureProfile);
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add<Context>().AsInterfaces().Scoped();

        // repositories
        container.Add<Func<Context, ITable<Package>>>(context => context.DotnetPackages).AsSelf().Singleton();
        container.Add<Func<Context, ITable<PackageDependency>>>(context => context.DotnetPackageDependencies).AsSelf().Singleton();
        container.Add<IPackageRepository<Server.Db.Dotnet.Models.Package, Server.Db.Dotnet.Models.PackageDependency>, PackageRepository<Server.Db.Dotnet.Models.Package, Server.Db.Dotnet.Models.PackageDependency, Package, PackageDependency, Context>>().AsSelf().Scoped();
    }

    private void ConfigureProfile(Profile p)
    {
        p.Map<Server.Db.Dotnet.Models.Package, Package>()
            .For(e => e.LowerName, e => e.Name.ToLower());
        p.Map<Server.Db.Dotnet.Models.PackageDependency, PackageDependency>()
            .Ignore(e => e.PackageId);
    }
}