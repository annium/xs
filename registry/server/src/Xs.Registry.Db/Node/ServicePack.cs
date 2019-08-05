using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db.Node
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<BaseServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddMapperConfiguration(ConfigureMapping);
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddScoped<INodeContext>(p => p.GetRequiredService<Context>());

            // repositories
            services.AddSingleton<Func<Context, ITable<Entities.Package>>>((Context context) => context.NodePackages);
            services.AddSingleton<Func<Context, ITable<Entities.PackageDependency>>>((Context context) => context.NodePackageDependencies);
            services.AddScoped<Shared.IPackageRepository<Package, PackageDependency>, Shared.PackageRepository<Package, PackageDependency, Entities.Package, Entities.PackageDependency, Context>>();
        }

        private void ConfigureMapping(MapperConfiguration cfg)
        {
            cfg.Map<Package, Entities.Package>()
                .Field(e => e.Name.ToLower(), e => e.LowerName);
            cfg.Map<PackageDependency, Entities.PackageDependency>()
                .Ignore(e => e.PackageId);
        }
    }
}