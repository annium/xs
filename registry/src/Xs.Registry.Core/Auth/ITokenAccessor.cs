using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Core.Auth
{
    public interface ITokenAccessor
    {
        ValueTuple<string, IActionResult> GetToken(HttpRequest request);
    }
}