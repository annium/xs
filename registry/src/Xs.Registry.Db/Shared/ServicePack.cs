using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Db.Shared
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<BaseServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<ISharedContext>(p => p.GetRequiredService<Context>());
        }
    }
}