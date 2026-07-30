using System;
using System.Linq;
using Annium.Core.DependencyInjection;
using Annium.Xs.Server.Shared.Auth;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Annium.Xs.Server.Shared.Internal.Auth;

internal class AuthorizationApplicationModelProvider : IApplicationModelProvider
{
    public int Order => -990;
    private readonly IServiceProvider _sp;

    public AuthorizationApplicationModelProvider(IServiceProvider sp)
    {
        _sp = sp;
    }

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
        //Intentionally empty
    }

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        foreach (var controllerModel in context.Result.Controllers)
            ProcessControllerModel(controllerModel);
    }

    private void ProcessControllerModel(ControllerModel controllerModel)
    {
        // AuthorizeAttribute permits AttributeTargets.Class, so a controller-level [Authorize] covers every
        // action on it. ActionModel.Attributes only carries the action's own attributes, never the declaring
        // controller's, so the controller has to be inspected here or a class-level attribute would leave
        // every action silently unauthorized.
        var controllerIsAuthorized = controllerModel.Attributes.OfType<AuthorizeAttribute>().Any();

        foreach (var actionModel in controllerModel.Actions)
            ProcessActionModel(actionModel, controllerIsAuthorized);
    }

    private void ProcessActionModel(ActionModel actionModel, bool controllerIsAuthorized)
    {
        //if no Authorize attribute on either the action or its controller - no filter needed
        if (!controllerIsAuthorized && !actionModel.Attributes.OfType<AuthorizeAttribute>().Any())
            return;

        actionModel.Filters.Add(_sp.Resolve<AuthorizationFilter>());
    }
}
