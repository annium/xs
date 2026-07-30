using System;
using System.Threading.Tasks;
using Annium.Net.Servers.Web;
using Annium.Testing;
using Annium.Xs.Server.Client;
using Xunit;

namespace Annium.Xs.Server.Client.Tests;

/// <summary>
/// Pins the behaviour of <see cref="ServerClientFactory.Create"/>: resolving a client from the
/// provider, assigning the given uri, and the transient lifetime of the resolved client.
/// </summary>
public class ServerClientFactoryTests : ClientTestBase
{
    public ServerClientFactoryTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<ServicePack>();
    }

    [Fact]
    public async Task Create_Uri_ResolvesClientWithUriAssigned()
    {
        // arrange
        await using var server = RunServer(
            (ctx, _) =>
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.Close();

                return Task.CompletedTask;
            }
        );
        var factory = Get<ServerClientFactory>();

        // act
        var client = factory.Create(server.HttpUri());

        // assert — a successful call proves the client was resolved and pointed at the server's uri
        await client.DeletePackageAsync("token", "pkg-a", "1.0.0");
        client.IsNotNull();
    }

    [Fact]
    public void Create_TwoCalls_ReturnsDistinctInstances()
    {
        // arrange — ServerClient is registered Transient in the ServicePack
        var factory = Get<ServerClientFactory>();

        // act
        var first = factory.Create(new Uri("http://127.0.0.1:1/"));
        var second = factory.Create(new Uri("http://127.0.0.1:2/"));

        // assert
        first.IsNot(second);
    }
}
