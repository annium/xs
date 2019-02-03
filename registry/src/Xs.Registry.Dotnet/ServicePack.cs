using System;
using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Dotnet.Db;

namespace Xs.Registry.Dotnet
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Core.ServicePack>();
            Add<BaseServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(
                new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("configuration", "dotnet.json"))
                .AddJsonFile(Path.Combine("configuration", "dotnet.override.json"), optional : true)
                .Build<Configuration>()
            );
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // repositories
            // services.AddSingleton<IPackageRepository<Package>, PackageRepository<Package, Db.Models.Package>>();
        }
    }
}