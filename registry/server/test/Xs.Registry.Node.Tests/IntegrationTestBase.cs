using Annium.Net.Http;

namespace Xs.Registry.Node.Tests
{
    public class IntegrationTestBase : Registry.Tests.IntegrationTestBase<Main.Startup>
    {
        protected IRequest Server => GetRequest<Startup>();

        public IntegrationTestBase() : base(container => container.UseServicePack<Main.TestServicePack>()) { }
    }
}