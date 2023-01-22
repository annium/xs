using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Server.Db.Repositories;
using Server.Domain.Models;
using Server.Shared.Auth;
using Server.Shared.Controllers;

namespace Server.Main.Auth;

internal class AuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ITimeProvider _timeProvider;

    private readonly Func<AuthorizationFilterContext, Task<ValueTuple<IActionResult, User>>>[] _authHandlers;

    public AuthorizationFilter(
        IServiceProvider serviceProvider,
        Access access
    )
    {
        _serviceProvider = serviceProvider;
        _timeProvider = serviceProvider.GetRequiredService<ITimeProvider>();
        _authHandlers = GetAuthHandlers(access).ToArray();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var result = await HandleAuthorizationAsync(context);
        if (result is not null)
            context.Result = result;
    }

    private async Task<IActionResult> HandleAuthorizationAsync(AuthorizationFilterContext context)
    {
        IActionResult result = null;
        User user = null;
        foreach (var handleAuthAsync in _authHandlers)
        {
            (result, user) = await handleAuthAsync(context);
            if (result is null)
                break;
        }

        // save user
        if (user is not null)
            context.ActionDescriptor.Properties[ServerController<User>.UserProperty] = user;

        return result;
    }

    private async Task<ValueTuple<IActionResult, User>> TryApiAuthorizationAsync(AuthorizationFilterContext context)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var tokenAccessor = scope.ServiceProvider.GetRequiredService<ITokenAccessor>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // try get token
            var (token, result) = tokenAccessor.GetToken(context.HttpContext.Request);
            if (result is not null)
                return (result, null);

            // try to find user
            var user = await userRepository.FindByApiTokenAsync(token);

            return user is null ? GetForbiddenResult("No user found with this token.") : (null, user);
        }
    }

    private async Task<ValueTuple<IActionResult, User>> TrySessionAuthorizationAsync(AuthorizationFilterContext context)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var userSessionRepository = scope.ServiceProvider.GetRequiredService<IUserSessionRepository>();
            var sessionManager = scope.ServiceProvider.GetRequiredService<ISessionManager>();

            // try get token
            var (token, result) = sessionManager.GetToken();
            if (result is not null)
                return (result, null);

            // try to find user session
            var session = await userSessionRepository.FindByTokenAsync(token);
            if (session is null)
                return GetForbiddenResult("Authorization failed. No identity found");

            // if token expired - failure
            if (session.Expires < _timeProvider.Now)
                return GetForbiddenResult("Authorization expired. Please login again");

            // refresh session
            await sessionManager.RefreshSession(session);

            return (null, await userRepository.GetById(session.UserId));
        }
    }

    private IEnumerable<Func<AuthorizationFilterContext, Task<ValueTuple<IActionResult, User>>>> GetAuthHandlers(Access access)
    {
        if (access.HasFlag(Access.Api))
            yield return TryApiAuthorizationAsync;
        if (access.HasFlag(Access.Session))
            yield return TrySessionAuthorizationAsync;
    }

    private ValueTuple<IActionResult, User> GetForbiddenResult(string error) =>
        (new ObjectResult(error) { StatusCode = (int) HttpStatusCode.Forbidden }, null);
}