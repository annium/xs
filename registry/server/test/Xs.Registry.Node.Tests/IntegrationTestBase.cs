using Annium.AspNetCore.IntegrationTesting;
using Annium.Net.Http;

namespace Xs.Registry.Node.Tests;

public class IntegrationTestBase : Registry.Tests.IntegrationTestBase<Main.Startup, Main.TestServicePack>
{
    protected IHttpRequest Server => AppFactory.GetHttpRequest();

    private IWebApplicationFactory AppFactory => GetAppFactory<Startup>(
        builder => builder.UseServicePack<TestServicePack>()
    );
}