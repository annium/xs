using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Shared.Helpers
{
    public class ServerController<TUser> : ControllerBase
    {
        public const string UserProperty = "serverUser";

        protected TUser GetUser()
        {
            if (ControllerContext.ActionDescriptor.Properties.TryGetValue(UserProperty, out var raw))
                return (TUser) raw;

            throw new InvalidOperationException($"User is not authenticated.");
        }

        protected IActionResult Created(object result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Created };

        protected IActionResult Forbidden(object result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };

        protected IActionResult ServerError(object result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.InternalServerError };
    }
}