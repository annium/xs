using System.IO;
using Annium.Extensions.Configuration;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Dotnet
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Abstract.ServicePack>();
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
    }
}