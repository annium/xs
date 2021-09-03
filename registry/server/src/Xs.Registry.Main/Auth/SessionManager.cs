using System;
using System.Net;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Xs.Registry.Db.Shared;

namespace Xs.Registry.Main.Auth
{
    internal class SessionManager : ISessionManager
    {
        private const string AuthCookieName = "AccessToken";

        private readonly Duration _lifeTime = Duration.FromDays(1);

        private readonly Duration _expirationBuffer = Duration.FromHours(12);


        private readonly ITimeProvider _timeProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IUserSessionRepository _userSessionRepository;

        public SessionManager(
            ITimeProvider timeProvider,
            IHttpContextAccessor httpContextAccessor,
            IUserSessionRepository userSessionRepository
        )
        {
            _timeProvider = timeProvider;
            _httpContextAccessor = httpContextAccessor;
            _userSessionRepository = userSessionRepository;
        }

        public (Guid, IActionResult) GetToken()
        {
            var cookies = _httpContextAccessor.HttpContext.Request.Cookies;

            if (!cookies.ContainsKey(AuthCookieName))
                return Fail(HttpStatusCode.Unauthorized, "Authorization required.");

            return Guid.TryParse(cookies[AuthCookieName], out var token) ? (token, null) : Fail(HttpStatusCode.Forbidden, "Invalid token passed");

            (Guid, IActionResult) Fail(HttpStatusCode statusCode, string message) =>
                (Guid.Empty, new ObjectResult(message) { StatusCode = (int) statusCode });
        }

        public async Task CreateSession(Guid userId)
        {
            // cleanup sessions
            var now = _timeProvider.Now;
            await _userSessionRepository.DeleteExpiredAsync(now);

            // create new one
            var token = Guid.NewGuid();
            var expires = now + Duration.FromDays(1);
            await _userSessionRepository.CreateAsync(new UserSession(token, userId, expires));

            // set cookie
            SetCookie(token, expires);
        }

        public async Task RefreshSession(UserSession session)
        {
            var now = _timeProvider.Now;

            // if session is expiring after expiration buffer - no need to prolongate right now
            if (session.Expires > now + _expirationBuffer)
                return;

            var expires = now + _lifeTime;

            // prolongate session
            await _userSessionRepository.ProlongateAsync(session.Token, expires);

            // set cookie
            SetCookie(session.Token, expires);
        }

        public async Task DeleteCurrentSession()
        {
            var (token, _) = GetToken();

            // cleanup sessions
            await _userSessionRepository.DeleteByTokenAsync(token);

            // delete cookie
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(AuthCookieName);
        }

        private void SetCookie(Guid token, Instant expires)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append(
                AuthCookieName,
                token.ToString(),
                new CookieOptions()
                {
                    Domain = _httpContextAccessor.HttpContext.Request.Host.Host,
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