using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;
using Annium.Xs.Server.Shared.Controllers;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Xs.Server.Shared.Internal.Auth;

internal class AuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var result = await HandleAuthorizationAsync(context);
        if (result is not null)
            context.Result = result;
    }

    private async Task<IActionResult?> HandleAuthorizationAsync(AuthorizationFilterContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var tokenAccessors = scope.ServiceProvider.Resolve<IEnumerable<ITokenAccessor>>();
        var userRepository = scope.ServiceProvider.Resolve<IUserRepository>();

        // try get token
        Guid token = default;
        IActionResult? result = null;
        var accessorRan = false;
        foreach (var tokenAccessor in tokenAccessors)
        {
            accessorRan = true;
            (token, result) = tokenAccessor.GetToken(context.HttpContext.Request);
            if (result is null)
                break;
        }

        // fail closed when no accessor ran at all: without this the loop leaves `result` null and `token`
        // at its default, and the lookup below would proceed as though an all-zero token had been legitimately
        // presented. Reachable only if the ITokenAccessor registration in ServicePack is ever lost, which is
        // exactly the case worth failing loudly on.
        if (!accessorRan)
            return GetForbiddenResult("No token accessor is registered; cannot authenticate the request.");

        if (result is not null)
            return result;

        // try to find user
        var user = await userRepository.TryFindByApiTokenAsync(token);
        if (user is null)
            return GetForbiddenResult("No user found with this token.");

        // save user
        context.ActionDescriptor.Properties[ServerController<User>.UserProperty] = user;

        return null;
    }

    private IActionResult GetForbiddenResult(string error) =>
        new ObjectResult(error) { StatusCode = (int)HttpStatusCode.Forbidden };
}
