using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Annium.Net.Servers.Web;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Annium.Xs.Server.Client.Clients;
using Xunit;

namespace Annium.Xs.Server.Client.Tests;

/// <summary>
/// Shared fixture for client tests. Wires up an <see cref="IHttpRequestFactory"/> backed by a
/// real <see cref="System.Net.Http.HttpClient"/> and JSON serialization, and provides a helper to
/// stand up a loopback <see cref="Annium.Net.Servers.Web"/> HTTP server for driving success and
/// failure branches of the typed clients.
/// </summary>
public abstract class ClientTestBase : TestBase
{
    /// <summary>
    /// The HTTP request factory resolved after <see cref="InitializeAsync"/> completes.
    /// </summary>
    private IHttpRequestFactory HttpRequestFactory
    {
        get => field ?? throw new InvalidOperationException("Accessed before InitializeAsync completed.");
        set;
    } = null!;

    protected ClientTestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(true);
            container.AddHttpRequestFactory(true);
        });
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        HttpRequestFactory = Get<IHttpRequestFactory>();
    }

    /// <summary>
    /// Creates a <see cref="MainClient"/> pointed at the given server's HTTP uri.
    /// </summary>
    protected MainClient CreateMainClient(IServer server) => CreateClient(new MainClient(HttpRequestFactory), server);

    /// <summary>
    /// Creates a <see cref="ServerClient"/> pointed at the given server's HTTP uri.
    /// </summary>
    protected ServerClient CreateServerClient(IServer server) =>
        CreateClient(new ServerClient(HttpRequestFactory), server);

    /// <summary>
    /// Assigns the started server's uri to the client via <see cref="ClientBase.SetUri"/>.
    /// </summary>
    private static TClient CreateClient<TClient>(TClient client, IServer server)
        where TClient : ClientBase
    {
        client.SetUri(server.HttpUri());

        return client;
    }

    /// <summary>
    /// Starts a loopback-only HTTP server (127.0.0.1, ephemeral port) with the given handler.
    /// </summary>
    protected IServer RunServer(Func<HttpListenerContext, CancellationToken, Task> handle)
    {
        var sp = Get<IServiceProvider>();
        var server = ServerBuilder
            .New(sp, host: "127.0.0.1")
            .WithHttpHandler(new DelegatingHttpHandler(handle))
            .Start();

        return server.NotNull();
    }
}

/// <summary>
/// HTTP handler that delegates to a provided callback — mirrors the pattern used by
/// Annium.Net.Servers.Web.Tests.
/// </summary>
file sealed class DelegatingHttpHandler : IHttpHandler
{
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handle;

    public DelegatingHttpHandler(Func<HttpListenerContext, CancellationToken, Task> handle)
    {
        _handle = handle;
    }

    public Task HandleAsync(HttpListenerContext ctx, CancellationToken ct) => _handle(ctx, ct);
}
