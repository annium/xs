using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Xs.Registry.Core.Auth
{
    internal class AuthorizationApplicationModelProvider : IApplicationModelProvider
    {
        public int Order { get; } = -990;

        private IAuthorizationFilterFactory authFilterFactory;

        public AuthorizationApplicationModelProvider(
            IAuthorizationFilterFactory authFilterFactory
        )
        {
            this.authFilterFactory = authFilterFactory;
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

            var filter = authFilterFactory.CreateFilter();

            actionModel.Filters.Add(filter);
        }
    }
}