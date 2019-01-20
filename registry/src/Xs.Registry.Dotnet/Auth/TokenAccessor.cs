using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Auth;

namespace Xs.Registry.Dotnet.Auth
{
    internal class TokenAccessor : ITokenAccessor
    {
        private const string TokenHeader = "X-NuGet-ApiKey";

        public ValueTuple<string, IActionResult> GetToken(HttpRequest request)
        {
            if (!request.Headers.ContainsKey(TokenHeader))
                return fail(HttpStatusCode.Unauthorized, "ApiKey authorization required");

            var token = request.Headers[TokenHeader].ToString();

            return (token, null);

            (string, IActionResult) fail(HttpStatusCode statusCode, string message) =>
                (null, new ObjectResult(message) { StatusCode = (int) statusCode });
        }
    }
}