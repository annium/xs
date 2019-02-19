using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Xs.Registry.Db.Shared;
using Xs.Registry.Shared.Auth;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Abstract.Auth
{
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IServiceProvider serviceProvider;

        public AuthorizationFilter(
            IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var result = await HandleAuthorizationAsync(context);
            if (result != null)
                context.Result = result;
        }

        private async Task<IActionResult> HandleAuthorizationAsync(AuthorizationFilterContext context)
        {
            using(var scope = serviceProvider.CreateScope())
            {
                var tokenAccessor = scope.ServiceProvider.GetRequiredService<ITokenAccessor>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                // try get token
                var(token, result) = tokenAccessor.GetToken(context.HttpContext.Request);
                if (result != null)
                    return result;

                // try to find user
                var user = await userRepository.FindByApiTokenAsync(token);
                if (user == null)
                    return GetForbiddenResult("No user found with this token.");

                // save user
                context.ActionDescriptor.Properties[ServerController<User>.UserProperty] = user;

                return null;
            }
        }

        private IActionResult GetForbiddenResult(string error) =>
            new ObjectResult(error) { StatusCode = (int) HttpStatusCode.Forbidden };
    }
}