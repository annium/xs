using Annium.Core.DependencyInjection;

namespace Server.Dotnet;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<BaseServicePack>();
        Add<Db.ServicePack>();
    }
}