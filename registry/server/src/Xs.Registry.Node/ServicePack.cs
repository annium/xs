using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Node;
using Xs.Registry.Db.Shared;
using Xs.Registry.Node.Payloads;
using Xs.Registry.Node.Storage;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Node
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Abstract.ServicePack>();
            Add<Db.Node.ServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(
                new ConfigurationBuilder()
                .AddJsonFile(Path.Combine("configuration", "node.json"))
                .AddJsonFile(Path.Combine("configuration", "node.override.json"), optional : true)
                .Build<Configuration>()
            );
        }

        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            // auth
            services.AddSingleton<ITokenAccessor>(new BearerTokenAccessor());

            // packages
            services.AddScoped<IPackageService<Package, PackageDependency, PackagePayload>, PackageService<Package, PackageDependency, PackagePayload>>();
            services.AddSingleton<IPayloadParser<PackagePayload, Package, PackageDependency>, PayloadParser>();
            services.AddSingleton<ProjectType>(Constants.ProjectType);

            // storage
            services.AddSingleton<Abstract.Packages.IPackageStorage, PackageStorage>();
            services.AddSingleton<Storage.IPackageStorage, PackageStorage>();

            // mapping
            services.AddMapper(provider);
        }
    }
}