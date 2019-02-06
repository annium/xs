using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Abstract.Auth;
using Xs.Registry.Dotnet.Storage;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Dotnet
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Abstract.ServicePack>();
            Add<Db.Dotnet.ServicePack>();
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

        public override void Register(IServiceCollection services, System.IServiceProvider provider)
        {
            // auth
            services.AddSingleton<ITokenAccessor>(new HeaderTokenAccessor("X-NuGet-ApiKey"));

            // storage
            services.AddSingleton<IPackageStorage, PackageStorage>();
            services.AddSingleton<ISymbolStorage, SymbolStorage>();

            // mapping
            services.AddAutoMapper(provider);
        }
    }
}