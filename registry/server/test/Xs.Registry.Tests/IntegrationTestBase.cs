using Annium.AspNetCore.IntegrationTesting;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;

namespace Xs.Registry.Tests
{
    public class IntegrationTestBase<TStartup, TServicePack> : IntegrationTest
    where TStartup : class
    where TServicePack : ServicePackBase, new()
    {
        protected IRequest Main => GetRequest<TStartup, TServicePack>();

        // protected async Task<UserPrivateView> RegisterUserAsync(
        //     string login = "demo",
        //     string password = "testtest",
        //     string email = "demo@demo.com"
        // )
        // {
        //     var payload = new UserPayload { Login = login, Password = password, Email = email };

        //     return await id.Put("/me").JsonContent(payload).AsAsync<UserPrivateView>();
        // }

        // protected async Task<ValueTuple<UserPrivateView, UserTokenView>> LoginUserAsync(
        //     string login = "demo",
        //     string password = "testtest",
        //     string email = "demo@demo.com"
        // )
        // {
        //     var user = await RegisterUserAsync(login, password, email);
        //     var payload = new UserLoginPayload { Login = login, Password = password };

        //     var tokens = await id.Post("/me/login").JsonContent(payload).AsAsync<UserTokenView>();

        //     return (user, tokens);
        // }
    }
}