using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Xs.Registry.Abstract.Auth
{
    public class BearerTokenAccessor : ITokenAccessor
    {
        public ValueTuple<Guid, IActionResult> GetToken(HttpRequest request)
        {
            if (!request.Headers.ContainsKey(HeaderNames.Authorization))
                return fail(HttpStatusCode.Unauthorized, "Bearer authorization required.");
            var authorization = request.Headers[HeaderNames.Authorization]
                .ToString().Split(' ').Select(e => e.Trim()).ToArray();
            if (authorization.Length != 2)
                return fail(HttpStatusCode.Forbidden, "Authorization format is invalid.");

            var(type, tokenString) = (authorization[0], authorization[1]);
            if (type != "Bearer")
                return fail(HttpStatusCode.Forbidden, "Bearer authorization required.");

            return Guid.TryParse(tokenString, out var token) ?
                (token, null) :
                fail(HttpStatusCode.Forbidden, "Invalid token passed");

            (Guid, IActionResult) fail(HttpStatusCode statusCode, string message) =>
                (Guid.Empty, new ObjectResult(message) { StatusCode = (int) statusCode });
        }
    }
}