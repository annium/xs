using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Abstract.Auth;
using Xs.Registry.Abstract.Storage;
using Xs.Registry.Abstract.Tools;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Abstract
{
    public class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Db.Shared.ServicePack>();
            Add<Shared.ServicePack>();
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // auth
            services.AddSingleton<Func<Access, AuthorizationFilter>>(sp => access => new AuthorizationFilter(sp));

            // storage
            services.AddSingleton<IStorageFactory, FileStorageFactory>();

            // tools
            services.AddSingleton<IRegistryConnectionManager, RegistryConnectionManager>();
        }
    }
}