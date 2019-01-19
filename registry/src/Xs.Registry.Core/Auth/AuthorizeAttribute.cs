using System;

namespace Xs.Registry.Core.Auth
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class AuthorizeAttribute : Attribute { }
}