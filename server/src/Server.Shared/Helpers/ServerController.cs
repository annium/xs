using System;
using Annium.AspNetCore.Extensions;
using Annium.Core.Mediator;

namespace Server.Shared.Helpers;

public class ServerController<TUser> : ServerController
{
    public const string UserProperty = "serverUser";

    protected ServerController(IMediator mediator, IServiceProvider sp) : base(mediator, sp)
    {
    }

    protected TUser GetUser()
    {
        if (ControllerContext.ActionDescriptor.Properties.TryGetValue(UserProperty, out var raw))
            return (TUser) raw;

        throw new InvalidOperationException($"User is not authenticated.");
    }
}