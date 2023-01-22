using System;

namespace Server.Domain.Enums;

[Flags]
public enum Permission
{
    None = 0,
    Read = 1,
    Publish = 2,
    Unpublish = 4,
}