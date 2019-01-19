using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Core.Auth;
using Xs.Registry.Shared.Auth;

namespace Xs.Registry.Shared
{
    public class BaseServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // auth
            services.AddSingleton<ITokenAccessor, TokenAccessor>();
        }
    }
}