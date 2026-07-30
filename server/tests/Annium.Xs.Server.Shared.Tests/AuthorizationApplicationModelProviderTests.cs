using System;
using System.Reflection;
using Annium;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Xs.Server.Shared.Auth;
using Annium.Xs.Server.Shared.Internal.Auth;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for the internal <see cref="Annium.Xs.Server.Shared.Internal.Auth.AuthorizationApplicationModelProvider"/>,
/// pinning which <see cref="ActionModel"/>s get an <see cref="AuthorizationFilter"/> attached. The
/// <see cref="ActionModel"/> instances built here only carry the attributes explicitly passed to their
/// constructor — mirroring how the real ASP.NET Core default provider builds them, from
/// <c>MethodInfo.GetCustomAttributes</c> (the method's own attributes only, never the declaring type's).
/// </summary>
public class AuthorizationApplicationModelProviderTests
{
    [Fact]
    public void OnProvidersExecuting_ActionHasAuthorizeAttribute_AddsAuthorizationFilter()
    {
        // arrange
        using var provider = BuildProvider();
        var applicationModelProvider = new AuthorizationApplicationModelProvider(provider);
        var (context, actionModel) = CreateContext(
            nameof(SampleController.SomeAction),
            actionAttributes: new object[] { new AuthorizeAttribute() }
        );

        // act
        applicationModelProvider.OnProvidersExecuting(context);

        // assert
        actionModel.Filters.Has(1);
        actionModel.Filters.At(0).Is(provider.Resolve<AuthorizationFilter>());
    }

    [Fact]
    public void OnProvidersExecuting_ActionHasNoAttributes_DoesNotAddFilter()
    {
        // arrange
        using var provider = BuildProvider();
        var applicationModelProvider = new AuthorizationApplicationModelProvider(provider);
        var (context, actionModel) = CreateContext(nameof(SampleController.SomeAction));

        // act
        applicationModelProvider.OnProvidersExecuting(context);

        // assert
        actionModel.Filters.IsEmpty();
    }

    [Fact]
    public void OnProvidersExecuting_OnlyControllerCarriesAuthorizeAttribute_AddsFilter()
    {
        // arrange — [Authorize] is usable at both Class and Method level (see its AttributeUsage), so a
        // controller-level attribute must cover every action on that controller. ActionModel.Attributes
        // only ever contains the action method's own attributes, never the declaring controller's, so the
        // provider has to inspect ControllerModel.Attributes as well.
        using var provider = BuildProvider();
        var applicationModelProvider = new AuthorizationApplicationModelProvider(provider);
        var (context, actionModel) = CreateContext(
            nameof(SampleController.SomeAction),
            controllerAttributes: new object[] { new AuthorizeAttribute() }
        );

        // act
        applicationModelProvider.OnProvidersExecuting(context);

        // assert
        actionModel.Filters.Has(1);
        actionModel.Filters[0].GetType().Is(typeof(AuthorizationFilter));
    }

    /// <summary>
    /// Registers <see cref="AuthorizationFilter"/> as a singleton so a single resolved instance can be
    /// compared by reference against whatever the provider under test added to
    /// <see cref="ActionModel.Filters"/>.
    /// </summary>
    private static IServiceProviderContainer BuildProvider()
    {
        var container = new ServiceContainer();
        container.Add<AuthorizationFilter>().AsSelf().Singleton();

        return container.BuildServiceProvider();
    }

    /// <summary>
    /// Builds an <see cref="ApplicationModelProviderContext"/> whose single controller/action pair is
    /// backed by <see cref="SampleController"/>, and returns both the context and the constructed
    /// <see cref="ActionModel"/> for direct post-call assertions.
    /// </summary>
    private static (ApplicationModelProviderContext Context, ActionModel ActionModel) CreateContext(
        string methodName,
        object[]? actionAttributes = null,
        object[]? controllerAttributes = null
    )
    {
        var controllerType = typeof(SampleController).GetTypeInfo();
        var method = typeof(SampleController).GetMethod(methodName).NotNull();

        var controllerModel = new ControllerModel(controllerType, controllerAttributes ?? Array.Empty<object>());
        var actionModel = new ActionModel(method, actionAttributes ?? Array.Empty<object>());
        controllerModel.Actions.Add(actionModel);

        var context = new ApplicationModelProviderContext(new[] { controllerType });
        context.Result.Controllers.Add(controllerModel);

        return (context, actionModel);
    }

    /// <summary>
    /// Test-local controller stand-in; only its method's presence/name matters — the provider under
    /// test never actually invokes it.
    /// </summary>
    private sealed class SampleController
    {
        public void SomeAction() { }
    }
}
