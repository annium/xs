using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Core.Helpers
{
    public class ServerController : ControllerBase
    {
        public const string UserProperty = "serverUser";

        protected User GetUser()
        {
            if (ControllerContext.ActionDescriptor.Properties.TryGetValue(UserProperty, out var raw))
                return (User) raw;

            throw new InvalidOperationException($"User is not authenticated.");
        }

        protected IActionResult Created(object result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Created };

        protected IActionResult Forbidden(object result) =>
            new ObjectResult(result) { StatusCode = (int) HttpStatusCode.Forbidden };
    }
}