using Annium.Net.Http;

namespace Xs.Registry.Dotnet.Tests
{
    public class IntegrationTestBase : Registry.Tests.IntegrationTestBase<Registry.Main.Startup<Registry.Main.TestServicePack>>
    {
        protected IRequest server => GetRequest<Startup<TestServicePack>>();
    }
}