using System;

namespace Annium.Xs.Server.Client.Models;

[Flags]
public enum Permission
{
    None = 0,
    Read = 1,
    Publish = 2,
    Unpublish = 4,
}
