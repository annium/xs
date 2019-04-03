using System;
using Annium.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Shared.Helpers
{
    public class ServerController<TUser> : ServerController
    {
        public const string UserProperty = "serverUser";

        protected TUser GetUser()
        {
            if (ControllerContext.ActionDescriptor.Properties.TryGetValue(UserProperty, out var raw))
                return (TUser) raw;

            throw new InvalidOperationException($"User is not authenticated.");
        }
    }
}