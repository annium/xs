using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Shared.Auth;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddRegistryAuthorization<TAuthorizationFilter>(this IServiceCollection services)
        where TAuthorizationFilter : IAsyncAuthorizationFilter
    {
        services.AddSingleton<IApplicationModelProvider, AuthorizationApplicationModelProvider<TAuthorizationFilter>>();

        return services;
    }
}