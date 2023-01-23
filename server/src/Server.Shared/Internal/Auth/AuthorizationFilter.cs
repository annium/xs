using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Server.Shared.Auth.TokenAccessors;
using Server.Shared.Controllers;
using Server.Shared.Domain.Models;
using Server.Shared.Repositories;

namespace Server.Shared.Internal.Auth;

public class AuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationFilter(
        IServiceProvider serviceProvider
    )
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
        Guid token = default(Guid);
        IActionResult? result = null;
        foreach (var tokenAccessor in tokenAccessors)
        {
            (token, result) = tokenAccessor.GetToken(context.HttpContext.Request);
            if (result is null)
                break;
        }

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
        new ObjectResult(error) { StatusCode = (int) HttpStatusCode.Forbidden };
}