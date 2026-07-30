using System;
using System.Net;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Client.Models;
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
        await using var server = RunJsonServer("\"secret-token\"");
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
        await using var server = RunErrorServer(HttpStatusCode.Unauthorized, "Unauthorized");
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
        await using var server = RunJsonServer("""{"Servers":{"main":"http://main.example.com/"}}""");
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
        await using var server = RunErrorServer(HttpStatusCode.InternalServerError, "Internal Server Error");
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
        await using var server = RunJsonServer("[]");
        var client = CreateMainClient(server);

        // act
        var results = await client.SearchAsync("token", "npm", "query");

        // assert
        results.IsEmpty();
    }

    [Fact]
    public async Task SearchAsync_Success_ReturnsPopulatedMetaPackage()
    {
        // arrange — exercises the MetaPackage DTO's field-by-field JSON mapping. Payload casing was
        // derived empirically from the actual serializer configured via AddSerializers().WithJson(true)
        // (default naming policy preserves C# property names verbatim; enums serialize as their member
        // name strings). NOTE: `Published` always round-trips to the Unix epoch — no NodaTime
        // System.Text.Json converter is registered for this pipeline, so an `Instant` serializes as `{}`
        // and deserializes back to `default(Instant)` regardless of the value assigned server-side.
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var ownerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        await using var server = RunJsonServer(
            $$"""
            [
                {
                    "Id": "{{id}}",
                    "Type": "npm",
                    "Name": "pkg-a",
                    "Version": "1.0.0",
                    "Description": "desc",
                    "Published": {},
                    "Downloads": 42,
                    "OwnerId": "{{ownerId}}",
                    "Owner": "owner",
                    "Permissions": [{ "Category": "Owner", "Permission": "Read" }]
                }
            ]
            """
        );
        var client = CreateMainClient(server);

        // act
        var results = await client.SearchAsync("token", "npm", "query");

        // assert
        results.Has(1);
        var pkg = results.At(0);
        pkg.Id.Is(id);
        pkg.Type.Is("npm");
        pkg.Name.Is("pkg-a");
        pkg.Version.Is("1.0.0");
        pkg.Description.Is("desc");
        pkg.Published.Is(default);
        pkg.Downloads.Is(42);
        pkg.OwnerId.Is(ownerId);
        pkg.Owner.Is("owner");
        pkg.Permissions.Has(1);
        var permission = pkg.Permissions.At(0);
        permission.Category.Is(PermissionCategory.Owner);
        permission.Permission.Is(Permission.Read);
    }

    [Fact]
    public async Task SearchAsync_NonSuccessResponse_ThrowsWithStatusCodeAndText()
    {
        // arrange
        await using var server = RunErrorServer(HttpStatusCode.BadRequest, "Bad Request");
        var client = CreateMainClient(server);

        // act
        var exception = await Wrap.It(async () => await client.SearchAsync("token", "npm", "query"))
            .ThrowsAsync<Exception>();

        // assert
        exception.Message.Is("Search failed with BadRequest (Bad Request).");
    }
}
