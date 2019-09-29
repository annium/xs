namespace Xs.Registry.Main.Tests
{
    public class IntegrationTestBase : Registry.Tests.IntegrationTestBase<Startup>
    {
        public IntegrationTestBase() : base(container => container.UseServicePack<TestServicePack>()) { }
    }
}