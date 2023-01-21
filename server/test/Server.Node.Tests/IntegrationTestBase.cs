using Annium.AspNetCore.IntegrationTesting;
using Annium.Net.Http;
using Xs.Registry.Tests;

namespace Xs.Registry.Node.Tests;

public class IntegrationTestBase : IntegrationTestBase<Main.Startup, Main.TestServicePack>
{
    protected IHttpRequest Server => AppFactory.GetHttpRequest();

    private IWebApplicationFactory AppFactory => GetAppFactory<Startup>(
        builder => builder.UseServicePack<TestServicePack>()
    );
}