using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Main.Auth
{
    internal class SessionManager : ISessionManager
    {
        private const string AuthCookieName = "AccessToken";

        private readonly Func<Instant> getInstant;

        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly IUserSessionRepository userSessionRepository;

        public SessionManager(
            Func<Instant> getInstant,
            IHttpContextAccessor httpContextAccessor,
            IUserSessionRepository userSessionRepository
        )
        {
            this.getInstant = getInstant;
            this.httpContextAccessor = httpContextAccessor;
            this.userSessionRepository = userSessionRepository;
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

        public async Task CreateSession(Guid userId)
        {
            // cleanup sessions
            var now = getInstant();
            await userSessionRepository.DeleteExpiredAsync(now);

            // create new one
            var token = Guid.NewGuid();
            var expires = now + Duration.FromDays(1);
            await userSessionRepository.CreateAsync(new UserSession(token, userId, expires));

            // set cookie
            SetCookie(token, expires);
        }

        public async Task RefreshSession(Guid token)
        {
            var expires = getInstant() + Duration.FromDays(1);

            // prolongate session
            await userSessionRepository.ProlongateAsync(token, expires);

            // set cookie
            SetCookie(token, expires);
        }

        public async Task DeleteCurrentSession()
        {
            var(token, _) = GetToken();

            // cleanup sessions
            await userSessionRepository.DeleteByTokenAsync(token);

            // delete cookie
            httpContextAccessor.HttpContext.Response.Cookies.Delete(AuthCookieName);
        }

        private void SetCookie(Guid token, Instant expires)
        {
            httpContextAccessor.HttpContext.Response.Cookies.Append(
                AuthCookieName,
                token.ToString(),
                new CookieOptions()
                {
                    Domain = httpContextAccessor.HttpContext.Request.Host.Host,
                        Path = "/",
                        Expires = expires.ToDateTimeOffset(),
                        Secure = false,
                        SameSite = SameSiteMode.None,
                        HttpOnly = true,
                }
            );
        }
    }
}