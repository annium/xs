using System;

namespace Server.Shared.Auth;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AuthorizeAttribute : Attribute
{
    public Access Access { get; }

    public AuthorizeAttribute(Access access)
    {
        Access = access;
    }
}