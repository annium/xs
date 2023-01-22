using System.IO;
using Annium.Core.DependencyInjection;
using Server.Main;

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
        container.AddConfiguration<Configuration>(x => x.AddYamlFile(Path.Combine("configuration", "main.yml")));
    }
}