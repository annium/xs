using Annium.Core.DependencyInjection;

namespace Server.Node;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<BaseServicePack>();
        Add<Db.ServicePack>();
    }
}