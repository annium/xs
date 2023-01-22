using System;

namespace Server.Shared.Auth;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AuthorizeSessionAttribute : AuthorizeAttribute
{
    public AuthorizeSessionAttribute() : base(Access.Session)
    {
    }
}