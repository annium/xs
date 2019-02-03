using Microsoft.AspNetCore.Mvc.Filters;

namespace Xs.Registry.Core.Auth
{
    internal interface IAuthorizationFilterFactory
    {
        IAsyncAuthorizationFilter CreateFilter(Access access);
    }
}