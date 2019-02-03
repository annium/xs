using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Xs.Registry.Core.Auth
{
    internal class AuthorizationFilterFactory : IAuthorizationFilterFactory
    {
        private readonly IServiceProvider provider;

        public AuthorizationFilterFactory(
            IServiceProvider provider
        )
        {
            this.provider = provider;
        }

        public IAsyncAuthorizationFilter CreateFilter(Access access)
        {
            switch (access)
            {
                case Access.Api:
                    return provider.GetRequiredService<ApiAuthorizationFilter>();
                case Access.Session:
                    return provider.GetRequiredService<SessionAuthorizationFilter>();
                default:
                    throw new NotImplementedException($"Access type {access} is not implemented");
            }
        }
    }
}