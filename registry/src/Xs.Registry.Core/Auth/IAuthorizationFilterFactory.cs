using Microsoft.AspNetCore.Mvc.Filters;

namespace Xs.Registry.Core.Auth
{
    public interface IAuthorizationFilterFactory
    {
        IAsyncAuthorizationFilter CreateFilter();
    }
}