using Microsoft.Extensions.DependencyInjection;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit
{
    public static class Extensions
    {
        public static IServiceCollection AddAuditRule<T, P>(this IServiceCollection services)
        where T : class, IAuditRule<P> where P : IProject
        {
            services.AddSingleton<IAuditRule<P>, T>();
            services.AddSingleton<IAuditRule, T>();

            return services;
        }
    }
}