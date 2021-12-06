using System;

namespace Xs.Registry.Shared.Auth;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class AuthorizeSessionAttribute : AuthorizeAttribute
{
    public AuthorizeSessionAttribute() : base(Access.Session) { }
}