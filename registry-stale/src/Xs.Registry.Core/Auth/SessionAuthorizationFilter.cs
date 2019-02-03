using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NodaTime;
using Xs.Registry.Core.Db;
using Xs.Registry.Core.Helpers;

namespace Xs.Registry.Core.Auth
{
    internal class SessionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly Func<Instant> getInstant;

        private readonly ISessionManager sessionManager;

        private readonly IUserRepository userRepository;

        private readonly IUserSessionRepository userSessionRepository;

        public SessionAuthorizationFilter(
            Func<Instant> getInstant,
            ISessionManager sessionManager,
            IUserRepository userRepository,
            IUserSessionRepository userSessionRepository
        )
        {
            this.getInstant = getInstant;
            this.sessionManager = sessionManager;
            this.userRepository = userRepository;
            this.userSessionRepository = userSessionRepository;
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
                return GetForbiddenResult("No session found with this token.");

            // if token expired - failure
            if (session.Expires < getInstant())
                return GetForbiddenResult("Authorization expired. Please login again");

            // refresh session
            await sessionManager.RefreshSession(session.Token);

            // save user
            var user = await userRepository.GetById(session.UserId);
            context.ActionDescriptor.Properties[ServerController.UserProperty] = user;

            return null;
        }

        private IActionResult GetForbiddenResult(string error) =>
            new ObjectResult(error) { StatusCode = (int) HttpStatusCode.Forbidden };
    }
}