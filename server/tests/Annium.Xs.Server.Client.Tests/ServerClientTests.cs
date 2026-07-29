using System;
using System.Net;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Xs.Server.Client.Tests;

/// <summary>
/// Pins the success and failure branches of <see cref="Clients.ServerClient"/>'s HTTP call sites
/// against a real loopback server.
/// </summary>
public class ServerClientTests : ClientTestBase
{
    public ServerClientTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task DeletePackageAsync_Success_DoesNotThrow()
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
        var client = CreateServerClient(server);

        // act
        await client.DeletePackageAsync("token", "pkg-a", "1.0.0");

        // assert — no exception means success; nothing further to observe from a void call
    }

    [Fact]
    public async Task DeletePackageAsync_NonSuccessResponse_ThrowsWithStatusCodeAndText()
    {
        // arrange
        await using var server = RunServer(
            (ctx, _) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                ctx.Response.StatusDescription = "Forbidden";
                ctx.Response.Close();

                return Task.CompletedTask;
            }
        );
        var client = CreateServerClient(server);

        // act
        var exception = await Wrap.It(async () => await client.DeletePackageAsync("token", "pkg-a", "1.0.0"))
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.Is("Delete package failed with Forbidden (Forbidden).");
    }
}
