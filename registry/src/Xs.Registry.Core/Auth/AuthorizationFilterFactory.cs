using Microsoft.AspNetCore.Mvc.Filters;
using Xs.Registry.Core.Repositories;

namespace Xs.Registry.Core.Auth
{
    internal class AuthorizationFilterFactory : IAuthorizationFilterFactory
    {
        private readonly ITokenAccessor tokenAccessor;

        private readonly IUserRepository userRepository;

        public AuthorizationFilterFactory(
            ITokenAccessor tokenAccessor,
            IUserRepository userRepository
        )
        {
            this.tokenAccessor = tokenAccessor;
            this.userRepository = userRepository;
        }

        public IAsyncAuthorizationFilter CreateFilter() =>
            new AuthorizationFilter(tokenAccessor, userRepository);
    }
}