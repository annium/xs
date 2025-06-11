using System;
using System.Linq;
using Annium.Core.DependencyInjection.Extensions;
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
        foreach (var actionModel in controllerModel.Actions)
            ProcessActionModel(actionModel);
    }

    private void ProcessActionModel(ActionModel actionModel)
    {
        var attribute = actionModel.Attributes.OfType<AuthorizeAttribute>().FirstOrDefault();

        //if no Authorize attribute - no filter needed
        if (attribute is null)
            return;

        actionModel.Filters.Add(_sp.Resolve<AuthorizationFilter>());
    }
}
