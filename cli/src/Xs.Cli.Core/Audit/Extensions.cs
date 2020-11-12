using Microsoft.Extensions.DependencyInjection;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit
{
    public static class Extensions
    {
        public static IServiceCollection AddAuditRule<T, Tp>(this IServiceCollection services)
        where T : class, IAuditRule<Tp> where Tp : IProject
        {
            services.AddSingleton<IAuditRule<Tp>, T>();
            services.AddSingleton<IAuditRule, T>();

            return services;
        }
    }
}