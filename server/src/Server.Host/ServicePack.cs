using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;

namespace Server.Host;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<BaseServicePack>();
        Add<Db.ServicePack>();
    }

    public override void Configure(IServiceContainer container)
    {
        container.AddConfiguration<Configuration>(x => x.AddYamlFile(Path.Combine("configuration", "main.yml")));
    }
}