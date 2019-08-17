using Annium.AspNetCore.IntegrationTesting;
using Annium.Net.Http;

namespace Xs.Registry.Node.Tests
{
    public class IntegrationTestBase : IntegrationTest
    {
        protected IRequest http => GetRequest<Startup<TestServicePack>>();
    }
}