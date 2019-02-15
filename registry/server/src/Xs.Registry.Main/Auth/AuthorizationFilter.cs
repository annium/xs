using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NodaTime;
using Xs.Registry.Db.Shared;
using Xs.Registry.Shared.Helpers;

namespace Xs.Registry.Main.Auth
{
    internal class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly Func<Instant> getInstant;

        private readonly IUserRepository userRepository;

        private readonly IUserSessionRepository userSessionRepository;

        private readonly ISessionManager sessionManager;

        public AuthorizationFilter(
            Func<Instant> getInstant,
            IUserRepository userRepository,
            IUserSessionRepository userSessionRepository,
            ISessionManager sessionManager
        )
        {
            this.getInstant = getInstant;
            this.userRepository = userRepository;
            this.userSessionRepository = userSessionRepository;
            this.sessionManager = sessionManager;
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

            // try to find user session
            var session = await userSessionRepository.FindByTokenAsync(token);
            if (session == null)
                return GetForbiddenResult("Authorization failed. No identity found");

            // if token expired - failure
            if (session.Expires < getInstant())
                return GetForbiddenResult("Authorization expired. Please login again");

            // refresh session
            await sessionManager.RefreshSession(session);

            // save user
            var user = await userRepository.GetById(session.UserId);
            context.ActionDescriptor.Properties[ServerController<User>.UserProperty] = user;

            return null;
        }

        private IActionResult GetForbiddenResult(string error) =>
            new ObjectResult(error) { StatusCode = (int) HttpStatusCode.Forbidden };
    }
}