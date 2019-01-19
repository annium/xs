using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Xs.Registry.Core.Auth;

namespace Xs.Registry.Node.Auth
{
    internal class TokenAccessor : ITokenAccessor
    {
        public ValueTuple<string, IActionResult> GetToken(HttpRequest request)
        {
            if (!request.Headers.ContainsKey(HeaderNames.Authorization))
                return fail(HttpStatusCode.Unauthorized, "Bearer authorization required");
            var authorization = request.Headers[HeaderNames.Authorization]
                .ToString().Split(' ').Select(e => e.Trim()).ToArray();
            if (authorization.Length != 2)
                return fail(HttpStatusCode.Forbidden, "Authorization format is invalid");

            var(type, token) = (authorization[0], authorization[1]);
            if (type != "Bearer")
                return fail(HttpStatusCode.Forbidden, "Bearer authorization required");

            return (token, null);

            (string, IActionResult) fail(HttpStatusCode statusCode, string message) =>
                (null, new ObjectResult(message) { StatusCode = (int) statusCode });
        }
    }
}