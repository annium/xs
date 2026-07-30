using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Shared.Controllers;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Internal.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;
using static Annium.Xs.Server.Shared.Tests.Helper;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="ServerController{TUser}.GetUser"/>, pinning both branches of its lookup against
/// <see cref="ActionDescriptor.Properties"/> — the same dictionary <see cref="AuthorizationFilter"/>
/// writes into under the same <see cref="ServerController{TUser}.UserProperty"/> key (see
/// <see cref="AuthorizationFilterTests"/>).
/// </summary>
public class ServerControllerTests
{
    [Fact]
    public void GetUser_UserPropertyPresent_ReturnsIt()
    {
        // arrange
        var user = new User("login", "hash", Guid.NewGuid());
        var controller = CreateController();
        controller.ControllerContext.ActionDescriptor.Properties[ServerController<User>.UserProperty] = user;

        // act
        var found = controller.InvokeGetUser();

        // assert
        found.Is(user);
    }

    [Fact]
    public void GetUser_UserPropertyAbsent_ThrowsInvalidOperationExceptionWithMessage()
    {
        // arrange
        var controller = CreateController();

        // act
        var exception = Wrap.It(() => controller.InvokeGetUser()).Throws<InvalidOperationException>();

        // assert
        exception.Message.Is("User is not authenticated.");
    }

    [Fact]
    public async Task GetUser_AfterAuthorizationFilterWritesUserProperty_ReturnsSameInstance()
    {
        // arrange — the round trip: AuthorizationFilter is what writes ServerController<TUser>.UserProperty
        // (see AuthorizationFilterTests), GetUser() is what reads it back. The two agree only by that
        // string key convention, so pin them together instead of each in isolation.
        var token = Guid.NewGuid();
        var user = new User("login", "hash", token);
        var userRepository = new FakeUserRepository();
        userRepository.Seed(user);
        var accessor = new FakeTokenAccessor(token);
        await using var provider = BuildProvider(userRepository, accessor);
        var filter = new AuthorizationFilter(provider);
        var controller = CreateController();
        var filterContext = new AuthorizationFilterContext(controller.ControllerContext, new List<IFilterMetadata>());

        // act
        await filter.OnAuthorizationAsync(filterContext);
        var found = controller.InvokeGetUser();

        // assert
        found.Is(user);
    }

    /// <summary>
    /// Builds a <see cref="TestServerController"/> whose <see cref="ControllerBase.ControllerContext"/> is
    /// a real, minimal one — a real <see cref="DefaultHttpContext"/> plus an empty
    /// <see cref="ActionDescriptor"/> whose <see cref="ActionDescriptor.Properties"/> dictionary tests write
    /// (or, in the round-trip test, <see cref="AuthorizationFilter"/> writes) the user into. Mirrors
    /// <c>AuthorizationFilterTests.CreateContext</c>.
    /// </summary>
    private static TestServerController CreateController()
    {
        var httpContext = new DefaultHttpContext();
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        return new TestServerController { ControllerContext = new ControllerContext(actionContext) };
    }

    /// <summary>
    /// Test-local subclass exposing the protected <see cref="ServerController{TUser}.GetUser"/> for direct
    /// invocation.
    /// </summary>
    private sealed class TestServerController : ServerController<User>
    {
        /// <summary>
        /// Invokes the protected <see cref="ServerController{TUser}.GetUser"/>.
        /// </summary>
        public User InvokeGetUser() => GetUser();
    }
}
