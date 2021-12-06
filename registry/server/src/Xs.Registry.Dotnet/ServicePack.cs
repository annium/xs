using Annium.Core.DependencyInjection;

namespace Xs.Registry.Dotnet;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<BaseServicePack>();
        Add<Db.BaseServicePack>();
    }
}