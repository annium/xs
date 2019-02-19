using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Node.Storage;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

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

            // storage
            services.AddSingleton<IPackageStorage, PackageStorage>();

            // mapping
            services.AddAutoMapper(provider);
        }
    }
}