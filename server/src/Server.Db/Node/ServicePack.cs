using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using LinqToDB;
using Server.Db.Node.Entities;
using Server.Db.Shared.Repositories;

namespace Server.Db.Node;

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
        container.Add<Func<Context, ITable<Package>>>(context => context.NodePackages).AsSelf().Singleton();
        container.Add<Func<Context, ITable<PackageDependency>>>(context => context.NodePackageDependencies).AsSelf().Singleton();
        container.Add<IPackageRepository<Server.Db.Node.Models.Package, Server.Db.Node.Models.PackageDependency>, PackageRepository<Server.Db.Node.Models.Package, Server.Db.Node.Models.PackageDependency, Package, PackageDependency, Context>>().AsSelf().Scoped();
    }

    private void ConfigureProfile(Profile p)
    {
        p.Map<Server.Db.Node.Models.Package, Package>()
            .For(e => e.LowerName, e => e.Name.ToLower());
        p.Map<Server.Db.Node.Models.PackageDependency, PackageDependency>()
            .Ignore(e => e.PackageId);
    }
}