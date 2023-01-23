using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Server.Shared.Internal.Auth;

namespace Server.Shared;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddAuthorization(this IServiceContainer container)
    {
        container.Add<IApplicationModelProvider, AuthorizationApplicationModelProvider>().Singleton();

        return container;
    }
}