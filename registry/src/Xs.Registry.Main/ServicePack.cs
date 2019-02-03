using Annium.Extensions.DependencyInjection;

namespace Xs.Registry.Main
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Db.Shared.ServicePack>();
        }
    }
}