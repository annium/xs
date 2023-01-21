using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Server.Shared.Internal.Auth;

namespace Server.Shared;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddRegistryAuthorization<TAuthorizationFilter>(this IServiceContainer container)
        where TAuthorizationFilter : IAsyncAuthorizationFilter
    {
        container.Add<IApplicationModelProvider, AuthorizationApplicationModelProvider<TAuthorizationFilter>>().Singleton();

        return container;
    }
}