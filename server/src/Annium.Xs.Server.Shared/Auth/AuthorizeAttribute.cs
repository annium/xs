using System;

namespace Annium.Xs.Server.Shared.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute { }
