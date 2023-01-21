using Annium.Core.DependencyInjection;

namespace Server.Dotnet;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<BaseServicePack>();
        Add<Xs.Registry.Db.BaseServicePack>();
    }
}