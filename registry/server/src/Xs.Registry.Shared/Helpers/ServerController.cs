using System;
using Annium.AspNetCore.Extensions;
using Annium.Core.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Xs.Registry.Shared.Helpers
{
    public class ServerController<TUser> : ServerController
    {
        public const string UserProperty = "serverUser";

        protected ServerController(IMediator mediator) : base(mediator) { }

        protected TUser GetUser()
        {
            if (ControllerContext.ActionDescriptor.Properties.TryGetValue(UserProperty, out var raw))
                return (TUser) raw;

            throw new InvalidOperationException($"User is not authenticated.");
        }
    }
}