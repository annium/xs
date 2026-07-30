using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Shared.Controllers;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Internal.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;
using static Annium.Xs.Server.Shared.Tests.Helper;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="AuthorizationFilter"/>, pinning its per-accessor token-resolution loop and the
/// 403 responses it produces when no matching user is found.
/// </summary>
public class AuthorizationFilterTests
{
    [Fact]
    public async Task OnAuthorizationAsync_SingleAccessorSucceedsAndUserFound_SetsUserPropertyAndLeavesResultNull()
    {
        // arrange
        var token = Guid.NewGuid();
        var user = new User("login", "hash", token);
        var userRepository = new FakeUserRepository();
        userRepository.Seed(user);
        var accessor = new FakeTokenAccessor(token);
        await using var provider = BuildProvider(userRepository, accessor);
        var filter = new AuthorizationFilter(provider);
        var context = CreateContext();

        // act
        await filter.OnAuthorizationAsync(context);

        // assert
        context.Result.IsNull();
        ((User)context.ActionDescriptor.Properties[ServerController<User>.UserProperty].IsNotNull()).Is(user);
    }

    [Fact]
    public async Task OnAuthorizationAsync_FirstAccessorFails_SetsResultAndNeverCallsUserRepository()
    {
        // arrange
        var failResult = new ObjectResult("nope") { StatusCode = (int)HttpStatusCode.Unauthorized };
        var accessor = new FakeTokenAccessor(Guid.Empty, failResult);
        var userRepository = new FakeUserRepository();
        await using var provider = BuildProvider(userRepository, accessor);
        var filter = new AuthorizationFilter(provider);
        var context = CreateContext();

        // act
        await filter.OnAuthorizationAsync(context);

        // assert
        context.Result.Is(failResult);
        userRepository.TryFindByApiTokenAsyncCallCount.Is(0);
    }

    [Fact]
    public async Task OnAuthorizationAsync_TwoAccessorsFirstSucceeds_SecondNeverInvoked()
    {
        // arrange
        var token = Guid.NewGuid();
        var user = new User("login", "hash", token);
        var userRepository = new FakeUserRepository();
        userRepository.Seed(user);
        var first = new FakeTokenAccessor(token);
        var second = new FakeTokenAccessor(Guid.NewGuid());
        await using var provider = BuildProvider(userRepository, first, second);
        var filter = new AuthorizationFilter(provider);
        var context = CreateContext();

        // act
        await filter.OnAuthorizationAsync(context);

        // assert
        context.Result.IsNull();
        first.CallCount.Is(1);
        second.CallCount.Is(0);
    }

    [Fact]
    public async Task OnAuthorizationAsync_FirstAccessorFailsSecondSucceeds_FallsThroughToSecondAndSucceeds()
    {
        // arrange — pinning the loop at AuthorizationFilter.cs:42-47 exactly as written: it does NOT
        // short-circuit the whole request on the first accessor's failure. It keeps iterating, and only
        // the LAST-evaluated accessor's outcome survives past the loop. Here accessor 2 succeeds, so its
        // token wins and accessor 1's earlier 401 is discarded entirely — not surfaced anywhere.
        var failResult = new ObjectResult("nope") { StatusCode = (int)HttpStatusCode.Unauthorized };
        var token = Guid.NewGuid();
        var user = new User("login", "hash", token);
        var userRepository = new FakeUserRepository();
        userRepository.Seed(user);
        var first = new FakeTokenAccessor(Guid.Empty, failResult);
        var second = new FakeTokenAccessor(token);
        await using var provider = BuildProvider(userRepository, first, second);
        var filter = new AuthorizationFilter(provider);
        var context = CreateContext();

        // act
        await filter.OnAuthorizationAsync(context);

        // assert
        context.Result.IsNull();
        first.CallCount.Is(1);
        second.CallCount.Is(1);
        ((User)context.ActionDescriptor.Properties[ServerController<User>.UserProperty].IsNotNull()).Is(user);
    }

    [Fact]
    public async Task OnAuthorizationAsync_NoAccessorsRegistered_FailsClosedWithoutQueryingRepository()
    {
        // arrange — with an empty IEnumerable<ITokenAccessor> the loop body never runs, so no token was
        // ever presented. That must fail closed rather than fall through to a lookup keyed by the C#
        // default (Guid.Empty) as though an all-zero token had been legitimately supplied.
        var userRepository = new FakeUserRepository();
        await using var provider = BuildProvider(userRepository);
        var filter = new AuthorizationFilter(provider);
        var context = CreateContext();

        // act
        await filter.OnAuthorizationAsync(context);

        // assert — rejected before any identity lookup happened
        userRepository.TryFindByApiTokenAsyncCallCount.Is(0);

        var objectResult = (ObjectResult)context.Result.IsNotNull();
        objectResult.StatusCode.Is((int)HttpStatusCode.Forbidden);
        objectResult.Value.Is("No token accessor is registered; cannot authenticate the request.");
    }

    [Fact]
    public async Task OnAuthorizationAsync_AccessorSucceedsButUserNotFound_ReturnsForbiddenWithMessage()
    {
        // arrange
        var token = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
        var accessor = new FakeTokenAccessor(token);
        await using var provider = BuildProvider(userRepository, accessor);
        var filter = new AuthorizationFilter(provider);
        var context = CreateContext();

        // act
        await filter.OnAuthorizationAsync(context);

        // assert
        var objectResult = (ObjectResult)context.Result.IsNotNull();
        objectResult.StatusCode.Is((int)HttpStatusCode.Forbidden);
        objectResult.Value.Is("No user found with this token.");
        userRepository.TryFindByApiTokenAsyncCallCount.Is(1);
    }

    /// <summary>
    /// Builds a minimal, real <see cref="AuthorizationFilterContext"/> — a real
    /// <see cref="DefaultHttpContext"/> plus an empty <see cref="ActionDescriptor"/> whose
    /// <see cref="ActionDescriptor.Properties"/> dictionary the filter writes the resolved user into.
    /// </summary>
    private static AuthorizationFilterContext CreateContext()
    {
        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }
}
