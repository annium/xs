using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Xs.Registry.Shared
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            services.AddSingleton(new LoggerConfiguration(LogLevel.Debug));
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(() => SystemClock.Instance.GetCurrentInstant());

            // helpers
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(p =>
            {
                var actionContext = p.GetRequiredService<IActionContextAccessor>().ActionContext;
                return p.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(actionContext);
            });

            services.AddConsoleLogger();
        }
    }
}