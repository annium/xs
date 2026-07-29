using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Xs.Server.Client.Tests;

/// <summary>
/// Pins the success and failure branches of <see cref="Clients.MainClient"/>'s HTTP call sites
/// against a real loopback server.
/// </summary>
public class MainClientTests : ClientTestBase
{
    public MainClientTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task LoginAsync_Success_ReturnsToken()
    {
        // arrange
        await using var server = RunServer(
            (ctx, _) =>
            {
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = 200;
                var data = Encoding.UTF8.GetBytes("\"secret-token\"");
                ctx.Response.OutputStream.Write(data);
                ctx.Response.Close();

                return Task.CompletedTask;
            }
        );
        var client = CreateMainClient(server);

        // act
        var token = await client.LoginAsync("user", "pass");

        // assert
        token.Is("secret-token");
    }

    [Fact]
    public async Task LoginAsync_NonSuccessResponse_ThrowsWithStatusCodeAndText()
    {
        // arrange
        await using var server = RunServer(
            (ctx, _) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                ctx.Response.StatusDescription = "Unauthorized";
                ctx.Response.Close();

                return Task.CompletedTask;
            }
        );
        var client = CreateMainClient(server);

        // act
        var exception = await Wrap.It(async () => await client.LoginAsync("user", "wrong-pass"))
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.Is("User login failed with Unauthorized (Unauthorized).");
    }

    [Fact]
    public async Task GetRegistryInfoAsync_Success_ReturnsRegistry()
    {
        // arrange
        await using var server = RunServer(
            (ctx, _) =>
            {
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = 200;
                var data = Encoding.UTF8.GetBytes("""{"Servers":{"main":"http://main.example.com/"}}""");
                ctx.Response.OutputStream.Write(data);
                ctx.Response.Close();

                return Task.CompletedTask;
            }
        );
        var client = CreateMainClient(server);

        // act
        var registry = await client.GetRegistryInfoAsync();

        // assert
        registry.Servers.Count.Is(1);
        registry.Servers["main"].Is(new Uri("http://main.example.com/"));
    }

    [Fact]
    public async Task GetRegistryInfoAsync_NonSuccessResponse_ThrowsWithStatusCodeAndText()
    {
        // arrange
        await using var server = RunServer(
            (ctx, _) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ctx.Response.StatusDescription = "Internal Server Error";
                ctx.Response.Close();

                return Task.CompletedTask;
            }
        );
        var client = CreateMainClient(server);

        // act
        var exception = await Wrap.It(async () => await client.GetRegistryInfoAsync()).ThrowsAsync<Exception>();

        // assert
        exception.Message.Is("Registry info fetch failed with InternalServerError (Internal Server Error).");
    }

    [Fact]
    public async Task SearchAsync_Success_ReturnsMetaPackages()
    {
        // arrange
        await using var server = RunServer(
            (ctx, _) =>
            {
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = 200;
                var data = Encoding.UTF8.GetBytes("[]");
                ctx.Response.OutputStream.Write(data);
                ctx.Response.Close();

                return Task.CompletedTask;
            }
        );
        var client = CreateMainClient(server);

        // act
        var results = await client.SearchAsync("token", "npm", "query");

        // assert
        results.IsEmpty();
    }

    [Fact]
    public async Task SearchAsync_NonSuccessResponse_ThrowsWithStatusCodeAndText()
    {
        // arrange
        await using var server = RunServer(
            (ctx, _) =>
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                ctx.Response.StatusDescription = "Bad Request";
                ctx.Response.Close();

                return Task.CompletedTask;
            }
        );
        var client = CreateMainClient(server);

        // act
        var exception = await Wrap.It(async () => await client.SearchAsync("token", "npm", "query"))
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.Is("Search failed with BadRequest (Bad Request).");
    }
}
