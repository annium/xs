using System;
using Annium.Extensions.DependencyInjection;
using AutoMapper.Configuration;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db.Dotnet
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<BaseServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton<MapperConfigurationExpression>(ConfigureMapping());
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<IDotnetContext>(p => p.GetRequiredService<Context>());

            // repositories
            services.AddSingleton<Func<Context, ITable<Entities.Package>>>((Context context) => context.DotnetPackages);
            services.AddSingleton<Shared.IPackageRepository<Package>, Shared.PackageRepository<Package, Entities.Package, Entities.PackageDependency, Context>>();
        }

        private MapperConfigurationExpression ConfigureMapping()
        {
            var cfg = new MapperConfigurationExpression();

            cfg.CreateMap<Package, Entities.Package>()
                .ForMember(p => p.LowerName, opt => opt.MapFrom(p => p.Name.ToLower()))
                .ReverseMap();
            cfg.CreateMap<PackageDependency, Entities.PackageDependency>()
                .ForMember(p => p.PackageId, opt => opt.Ignore())
                .ReverseMap();

            return cfg;
        }
    }
}