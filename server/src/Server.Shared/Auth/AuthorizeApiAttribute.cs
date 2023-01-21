using System;

namespace Server.Shared.Auth;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class AuthorizeApiAttribute : AuthorizeAttribute
{
    public AuthorizeApiAttribute() : base(Access.Api)
    {
    }
}