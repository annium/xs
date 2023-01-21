using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Shared.Auth;

public interface ITokenAccessor
{
    ValueTuple<Guid, IActionResult?> GetToken(HttpRequest request);
}