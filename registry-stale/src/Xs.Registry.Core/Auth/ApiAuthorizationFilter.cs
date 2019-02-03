using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Db;

namespace Xs.Registry.Core.Auth
{
    internal class ApiAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly ITokenAccessor tokenAccessor;

        private readonly IUserRepository userRepository;

        public ApiAuthorizationFilter(
            ITokenAccessor tokenAccessor,
            IUserRepository userRepository
        )
        {
            this.tokenAccessor = tokenAccessor;
            this.userRepository = userRepository;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var result = await HandleAuthorizationAsync(context);
            if (result != null)
                context.Result = result;
        }

        private async Task<IActionResult> HandleAuthorizationAsync(AuthorizationFilterContext context)
        {
            // try get token
            var(token, result) = tokenAccessor.GetToken(context.HttpContext.Request);
            if (result != null)
                return result;

            // try to find user
            var user = await userRepository.FindByApiTokenAsync(token);
            if (user == null)
                return GetForbiddenResult("No user found with this token.");

            // save user
            context.ActionDescriptor.Properties[ServerController.UserProperty] = user;

            return null;
        }

        private IActionResult GetForbiddenResult(string error) =>
            new ObjectResult(error) { StatusCode = (int) HttpStatusCode.Forbidden };
    }
}