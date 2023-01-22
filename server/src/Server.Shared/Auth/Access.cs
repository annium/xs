using System;

namespace Server.Shared.Auth;

[Flags]
public enum Access
{
    Api = 1,
    Session = 2,
}