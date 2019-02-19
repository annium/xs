using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Shared.Auth
{
    public class HeaderTokenAccessor : ITokenAccessor
    {
        private readonly string header;

        public HeaderTokenAccessor(string header)
        {
            this.header = header;
        }

        public ValueTuple<Guid, IActionResult> GetToken(HttpRequest request)
        {
            if (!request.Headers.ContainsKey(header))
                return fail(HttpStatusCode.Unauthorized, $"Authorization with '{header}' header required.");

            return Guid.TryParse(request.Headers[header].ToString(), out var token) ?
                (token, null) :
                fail(HttpStatusCode.Forbidden, "Invalid token passed");

            (Guid, IActionResult) fail(HttpStatusCode statusCode, string message) =>
                (Guid.Empty, new ObjectResult(message) { StatusCode = (int) statusCode });
        }
    }
}