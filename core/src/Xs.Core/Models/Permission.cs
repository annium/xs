using System;

namespace Xs.Core.Models
{
    [Flags]
    public enum Permission
    {
        None = 0,
        Read = 1,
        Publish = 2,
        Unpublish = 4,
        Republish = 8,
    }
}