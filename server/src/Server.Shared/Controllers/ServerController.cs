using System;
using Microsoft.AspNetCore.Mvc;

namespace Server.Shared.Controllers;

public class ServerController<TUser> : ControllerBase
{
    public const string UserProperty = "serverUser";

    protected TUser GetUser()
    {
        if (ControllerContext.ActionDescriptor.Properties.TryGetValue(UserProperty, out var raw))
            return (TUser)raw!;

        throw new InvalidOperationException($"User is not authenticated.");
    }
}
