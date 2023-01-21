using System;

namespace Server.Db.Shared.Models;

[Flags]
public enum Permission
{
    None = 0,
    Read = 1,
    Publish = 2,
    Unpublish = 4,
}