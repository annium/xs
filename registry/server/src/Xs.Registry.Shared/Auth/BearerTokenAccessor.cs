using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Xs.Registry.Shared.Auth
{
    public class BearerTokenAccessor : ITokenAccessor
    {
        public ValueTuple<Guid, IActionResult> GetToken(HttpRequest request)
        {
            if (!request.Headers.ContainsKey(HeaderNames.Authorization))
                return Fail(HttpStatusCode.Unauthorized, "Bearer authorization required.");
            var authorization = request.Headers[HeaderNames.Authorization]
                .ToString().Split(' ').Select(e => e.Trim()).ToArray();
            if (authorization.Length != 2)
                return Fail(HttpStatusCode.Forbidden, "Authorization format is invalid.");

            var(type, tokenString) = (authorization[0], authorization[1]);
            if (type != "Bearer")
                return Fail(HttpStatusCode.Forbidden, "Bearer authorization required.");

            return Guid.TryParse(tokenString, out var token) ?
                (token, null) :
                Fail(HttpStatusCode.Forbidden, "Invalid token passed");

            (Guid, IActionResult) Fail(HttpStatusCode statusCode, string message) =>
                (Guid.Empty, new ObjectResult(message) { StatusCode = (int) statusCode });
        }
    }
}