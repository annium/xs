using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Annium.Xs.Server.Shared.Auth.TokenAccessors;

public interface ITokenAccessor
{
    ValueTuple<Guid, IActionResult?> GetToken(HttpRequest request);
}
