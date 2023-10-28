using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Shared.Auth.TokenAccessors;

public interface ITokenAccessor
{
    ValueTuple<Guid, IActionResult?> GetToken(HttpRequest request);
}
