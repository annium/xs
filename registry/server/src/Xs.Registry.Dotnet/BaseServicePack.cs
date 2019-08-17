using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Abstract.Packages;
using Xs.Registry.Db.Dotnet;
using Xs.Registry.Db.Shared;
using Xs.Registry.Dotnet.Payloads;
using Xs.Registry.Dotnet.Storage;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Dotnet
{
    internal class BaseServicePack : ServicePackBase
    {
        public BaseServicePack()
        {
            Add<Abstract.ServicePack>();
            Add<Db.Dotnet.ServicePack>();
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(new Configuration());
        }

        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            // auth
            services.AddSingleton<ITokenAccessor>(new HeaderTokenAccessor("X-NuGet-ApiKey"));
            services.AddSingleton<ITokenAccessor>(new BearerTokenAccessor());

            // packages
            services.AddScoped<IPackageService<Package, PackageDependency, PackagePayload>, PackageService<Package, PackageDependency, PackagePayload>>();
            services.AddSingleton<IPayloadParser<PackagePayload, Package, PackageDependency>, PayloadParser>();
            services.AddSingleton<ProjectType>(Constants.ProjectType);

            // storage
            services.AddSingleton<Abstract.Packages.IPackageStorage, PackageStorage>();
            services.AddSingleton<Storage.IPackageStorage, PackageStorage>();
            services.AddSingleton<ISymbolStorage, SymbolStorage>();

            // mapping
            services.AddMapper(provider);
        }
    }
}