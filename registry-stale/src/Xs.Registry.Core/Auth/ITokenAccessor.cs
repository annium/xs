using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Core.Auth
{
    public interface ITokenAccessor
    {
        ValueTuple<Guid, IActionResult> GetToken(HttpRequest request);
    }
}