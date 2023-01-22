using System;

namespace Server.Shared.Auth;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AuthorizeApiAttribute : AuthorizeAttribute
{
    public AuthorizeApiAttribute() : base(Access.Api)
    {
    }
}