using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using LinqToDB;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Db.Node
{
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
            container.Add<Func<Context, ITable<Entities.Package>>>(context => context.NodePackages).AsSelf().Singleton();
            container.Add<Func<Context, ITable<Entities.PackageDependency>>>(context => context.NodePackageDependencies).AsSelf().Singleton();
            container.Add<IPackageRepository<Package, PackageDependency>, PackageRepository<Package, PackageDependency, Entities.Package, Entities.PackageDependency, Context>>().AsSelf().Scoped();
        }

        private void ConfigureProfile(Profile p)
        {
            p.Map<Package, Entities.Package>()
                .For(e => e.LowerName, e => e.Name.ToLower());
            p.Map<PackageDependency, Entities.PackageDependency>()
                .Ignore(e => e.PackageId);
        }
    }
}