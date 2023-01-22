using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;

namespace Server.Host;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<BaseServicePack>();
    }

    public override void Configure(IServiceContainer container)
    {
        container.AddConfiguration(new WebHostConfiguration());
        container.AddConfiguration<Main.Configuration>(x => x.AddYamlFile(Path.Combine("configuration", "main.yml")));
    }
}