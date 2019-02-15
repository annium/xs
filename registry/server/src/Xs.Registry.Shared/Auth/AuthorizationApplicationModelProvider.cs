using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Xs.Registry.Shared.Auth
{
    internal class AuthorizationApplicationModelProvider<TAuthorizationFilter> : IApplicationModelProvider
    where TAuthorizationFilter : IAsyncAuthorizationFilter
    {
        public int Order { get; } = -990;

        private readonly TAuthorizationFilter authorizationFilter;

        public AuthorizationApplicationModelProvider(
            TAuthorizationFilter authorizationFilter
        )
        {
            this.authorizationFilter = authorizationFilter;
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
            if (attribute == null)
                return;

            actionModel.Filters.Add(authorizationFilter);
        }
    }
}