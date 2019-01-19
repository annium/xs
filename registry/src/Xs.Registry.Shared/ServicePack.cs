using Annium.Extensions.DependencyInjection;

namespace Xs.Registry.Shared
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Core.BaseServicePack>();
            Add<Core.ServicePack>();
            Add<BaseServicePack>();
        }
    }
}