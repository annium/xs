using System;

namespace Xs.Registry.Shared.Auth;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class AuthorizeAttribute : Attribute
{
    public Access Access { get; }

    public AuthorizeAttribute(Access access)
    {
        Access = access;
    }
}