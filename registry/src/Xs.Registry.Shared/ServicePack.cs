using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Xs.Registry.Shared
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(() => SystemClock.Instance.GetCurrentInstant());

            // helpers
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}