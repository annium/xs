using Microsoft.Extensions.DependencyInjection;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit
{
    public static class Extensions
    {
        public static IServiceCollection AddAuditRule<T, TP>(this IServiceCollection services)
        where T : class, IAuditRule<TP> where TP : IProject
        {
            services.AddSingleton<IAuditRule<TP>, T>();
            services.AddSingleton<IAuditRule, T>();

            return services;
        }
    }
}