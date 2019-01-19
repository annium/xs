using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Core.Auth
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddRegistryAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IApplicationModelProvider, AuthorizationApplicationModelProvider>();

            return services;
        }
    }
}