using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db.Dotnet
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddMapperConfiguration(ConfigureMapping);
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddScoped<IDotnetContext>(p => p.GetRequiredService<Context>());

            // repositories
            services.AddSingleton<Func<Context, ITable<Entities.Package>>>((Context context) => context.DotnetPackages);
            services.AddSingleton<Func<Context, ITable<Entities.PackageDependency>>>((Context context) => context.DotnetPackageDependencies);
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