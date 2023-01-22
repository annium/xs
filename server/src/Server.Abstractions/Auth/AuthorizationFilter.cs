using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Server.Db.Shared.Repositories;
using Server.Domain.Models;
using Server.Shared.Auth;
using Server.Shared.Controllers;

namespace Server.Abstractions.Auth;

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
        using var scope = _serviceProvider.CreateScope();

        var tokenAccessors = scope.ServiceProvider.GetRequiredService<IEnumerable<ITokenAccessor>>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

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
        var user = await userRepository.FindByApiTokenAsync(token);
        if (user is null)
            return GetForbiddenResult("No user found with this token.");

        // save user
        context.ActionDescriptor.Properties[ServerController<User>.UserProperty] = user;

        return null;
    }

    private IActionResult GetForbiddenResult(string error) =>
        new ObjectResult(error) { StatusCode = (int) HttpStatusCode.Forbidden };
}