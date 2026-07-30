using System;
using System.Threading.Tasks;
using Annium.Net.Servers.Web;
using Annium.Testing;
using Annium.Xs.Server.Client;
using Xunit;

namespace Annium.Xs.Server.Client.Tests;

/// <summary>
/// Pins the behaviour of <see cref="MainClientFactory.Create"/>: resolving a client from the
/// provider, assigning the given uri, and the transient lifetime of the resolved client.
/// </summary>
public class MainClientFactoryTests : ClientTestBase
{
    public MainClientFactoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<ServicePack>();
    }

    [Fact]
    public async Task Create_Uri_ResolvesClientWithUriAssigned()
    {
        // arrange
        await using var server = RunJsonServer("""{"Servers":{}}""");
        var factory = Get<MainClientFactory>();

        // act
        var client = factory.Create(server.HttpUri());
        var registry = await client.GetRegistryInfoAsync();

        // assert — a successful call proves the client was resolved and pointed at the server's uri
        client.IsNotNull();
        registry.Servers.Count.Is(0);
    }

    [Fact]
    public void Create_TwoCalls_ReturnsDistinctInstances()
    {
        // arrange — MainClient is registered Transient in the ServicePack
        var factory = Get<MainClientFactory>();

        // act
        var first = factory.Create(new Uri("http://127.0.0.1:1/"));
        var second = factory.Create(new Uri("http://127.0.0.1:2/"));

        // assert
        first.IsNot(second);
    }
}
