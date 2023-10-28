using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Shared.Auth.TokenAccessors;

public class HeaderTokenAccessor : ITokenAccessor
{
    private readonly string _header;

    public HeaderTokenAccessor(string header)
    {
        _header = header;
    }

    public ValueTuple<Guid, IActionResult?> GetToken(HttpRequest request)
    {
        if (!request.Headers.ContainsKey(_header))
            return Fail(HttpStatusCode.Unauthorized, $"Authorization with '{_header}' header required.");

        return Guid.TryParse(request.Headers[_header].ToString(), out var token)
            ? (token, null)
            : Fail(HttpStatusCode.Forbidden, "Invalid token passed");
    }

    private (Guid, IActionResult) Fail(HttpStatusCode statusCode, string message) =>
        (Guid.Empty, new ObjectResult(message) { StatusCode = (int)statusCode });
}
