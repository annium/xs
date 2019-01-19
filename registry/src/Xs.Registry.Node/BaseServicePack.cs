using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Core.Auth;
using Xs.Registry.Node.Auth;
using Xs.Registry.Node.Storage;

namespace Xs.Registry.Node
{
    public class BaseServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // auth
            services.AddSingleton<ITokenAccessor, TokenAccessor>();

            // storage
            services.AddSingleton<IPackageStorage, PackageStorage>();
        }
    }
}