using System;

namespace Xs.Registry.Shared.Auth;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class AuthorizeApiAttribute : AuthorizeAttribute
{
    public AuthorizeApiAttribute() : base(Access.Api)
    {
    }
}