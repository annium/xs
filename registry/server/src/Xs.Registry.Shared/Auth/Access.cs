using System;

namespace Xs.Registry.Shared.Auth;

[Flags]
public enum Access
{
    Api = 1,

    Session = 2,
}