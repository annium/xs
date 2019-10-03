using Annium.Net.Http;

namespace Xs.Registry.Dotnet.Tests
{
    public class IntegrationTestBase : Registry.Tests.IntegrationTestBase<Main.Startup, Main.TestServicePack>
    {
        protected IRequest Server => GetRequest<Startup, TestServicePack>();
    }
}