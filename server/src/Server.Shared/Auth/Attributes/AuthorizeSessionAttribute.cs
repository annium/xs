using System;

namespace Server.Shared.Auth.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AuthorizeSessionAttribute : AuthorizeAttribute
{
    public AuthorizeSessionAttribute() : base(Access.Session)
    {
    }
}