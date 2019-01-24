using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xs.Registry.Core.Helpers;
using Xs.Registry.Core.Repositories;

namespace Xs.Registry.Core.Auth
{
    internal class SessionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly Func<DateTime> getTime;

        private readonly ISessionManager sessionManager;

        private readonly IUserRepository userRepository;

        public SessionAuthorizationFilter(
            Func<DateTime> getTime,
            ISessionManager sessionManager,
            IUserRepository userRepository
        )
        {
            this.getTime = getTime;
            this.sessionManager = sessionManager;
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
            var(token, result) = sessionManager.GetToken();
            if (result != null)
                return result;

            // try to find user
            var user = await userRepository.FindBySessionTokenAsync(token);
            if (user == null)
                return GetForbiddenResult("No user found with this token.");

            var userSession = user.Sessions.First(s => s.Token == token);

            // if token expired - failure
            if (userSession.Expires < getTime())
                return GetForbiddenResult("Authorization expired. Please login again");

            // save session to get it prolongated
            await sessionManager.SaveSession(user, userSession.Token);

            // save user
            context.ActionDescriptor.Properties[ServerController.UserProperty] = user;

            return null;
        }

        private IActionResult GetForbiddenResult(string error) =>
            new ObjectResult(error) { StatusCode = (int) HttpStatusCode.Forbidden };
    }
}