using Annium.AspNetCore.IntegrationTesting;
using Annium.Net.Http;

namespace Xs.Registry.Tests
{
    public class IntegrationTestBase<TStartup> : IntegrationTest where TStartup : class
    {
        protected IRequest main => GetRequest<TStartup>();
    }
}