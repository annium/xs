using System;

namespace Server.Shared.Auth.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AuthorizeApiAttribute : AuthorizeAttribute
{
    public AuthorizeApiAttribute() : base(Access.Api)
    {
    }
}