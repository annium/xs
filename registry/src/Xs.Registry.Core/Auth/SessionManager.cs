using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Models;
using Xs.Registry.Core.Db;

namespace Xs.Registry.Core.Auth
{
    internal class SessionManager : ISessionManager
    {
        private const string AuthCookieName = "AccessToken";

        private readonly Func<DateTime> getTime;

        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly IUserRepository userRepository;

        public SessionManager(
            Func<DateTime> getTime,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository
        )
        {
            this.getTime = getTime;
            this.httpContextAccessor = httpContextAccessor;
            this.userRepository = userRepository;
        }

        public(Guid, IActionResult) GetToken()
        {
            var cookies = httpContextAccessor.HttpContext.Request.Cookies;

            if (!cookies.ContainsKey(AuthCookieName))
                return fail(HttpStatusCode.Unauthorized, "Authorization required.");

            return Guid.TryParse(cookies[AuthCookieName], out var token) ?
                (token, null) :
                fail(HttpStatusCode.Forbidden, "Invalid token passed");

            (Guid, IActionResult) fail(HttpStatusCode statusCode, string message) =>
                (Guid.Empty, new ObjectResult(message) { StatusCode = (int) statusCode });
        }

        public async Task SaveSession(User user, Guid token)
        {
            var now = getTime();
            var expires = now + TimeSpan.FromDays(1);

            // renew sessions
            user.Sessions.RemoveAll(s => s.Token == token || s.Expires < now);
            user.Sessions.Add(new UserSession(token, expires));

            await userRepository.SaveAsync(user);

            // set cookie
            httpContextAccessor.HttpContext.Response.Cookies.Append(
                AuthCookieName,
                token.ToString(),
                new CookieOptions()
                {
                    Domain = httpContextAccessor.HttpContext.Request.Host.Host,
                        Path = "/",
                        Expires = expires,
                        Secure = false,
                        SameSite = SameSiteMode.None,
                        HttpOnly = true,
                }
            );
        }

        public async Task DeleteSession(User user)
        {
            var(token, _) = GetToken();
            var now = getTime();
            user.Sessions.RemoveAll(s => s.Token == token || s.Expires < now);

            await userRepository.SaveAsync(user);

            // delete cookie
            httpContextAccessor.HttpContext.Response.Cookies.Delete(AuthCookieName);
        }
    }
}