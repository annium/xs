using Annium.Net.Http;

namespace Xs.Registry.Node.Tests
{
    public class IntegrationTestBase : Registry.Tests.IntegrationTestBase<Main.Startup, Main.TestServicePack>
    {
        protected IHttpRequest Server => GetRequest<Startup>(
            builder => builder.UseServicePack<TestServicePack>()
        );
    }
}