using Annium.AspNetCore.IntegrationTesting;
using Annium.Net.Http;

namespace Xs.Registry.Dotnet.Tests
{
    public class IntegrationTestBase : IntegrationTest
    {
        protected IRequest http => GetRequest<Startup<TestServicePack>>();
    }
}