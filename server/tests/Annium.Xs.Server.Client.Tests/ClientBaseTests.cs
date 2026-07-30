using System;
using Annium.Testing;
using Annium.Xs.Server.Client.Clients;
using Xunit;

namespace Annium.Xs.Server.Client.Tests;

/// <summary>
/// Pins the current behaviour of <see cref="ClientBase.SetUri"/>: first-call assignment,
/// the "already assigned" guard, and cascading to child clients.
/// </summary>
public class ClientBaseTests : TestBase
{
    public ClientBaseTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public void SetUri_FirstCall_AssignsUri()
    {
        // arrange
        var client = new ProbeClient();
        var uri = new Uri("http://example.com/");

        // act
        client.SetUri(uri);

        // assert
        client.CurrentUri.Is(uri);
    }

    [Fact]
    public void SetUri_UriAlreadyAssignedToNonLoopbackHost_Throws()
    {
        // arrange
        var client = new ProbeClient();
        client.SetUri(new Uri("http://example.com/"));

        // act
        var exception = Wrap.It(() => client.SetUri(new Uri("http://another.example.com/")))
            .Throws<InvalidOperationException>();

        // assert
        exception.Message.Is("Uri already assigned.");
    }

    [Fact]
    public void SetUri_UriAlreadyAssignedToLoopbackHost_Throws()
    {
        // arrange — assignment must be tracked explicitly, not inferred from whether the
        // currently-assigned uri happens to be loopback (e.g. the loopback test servers used
        // throughout this suite), so a second SetUri call always throws regardless of host.
        var client = new ProbeClient();
        client.SetUri(new Uri("http://127.0.0.1:12345/"));
        var secondUri = new Uri("http://127.0.0.1:54321/");

        // act
        var exception = Wrap.It(() => client.SetUri(secondUri)).Throws<InvalidOperationException>();

        // assert
        exception.Message.Is("Uri already assigned.");
    }

    [Fact]
    public void SetUri_WithChildren_CascadesToAllChildren()
    {
        // arrange — MainClient/ServerClient pass zero children to the base constructor, so this
        // is pinned via a purpose-built subclass with children.
        var child1 = new ProbeClient();
        var child2 = new ProbeClient();
        var parent = new ParentClient(child1, child2);
        var uri = new Uri("http://cascade.example.com/");

        // act
        parent.SetUri(uri);

        // assert
        parent.CurrentUri.Is(uri);
        child1.CurrentUri.Is(uri);
        child2.CurrentUri.Is(uri);
    }

    /// <summary>
    /// Minimal <see cref="ClientBase"/> subclass exposing the protected <c>Uri</c> field for assertions.
    /// </summary>
    private sealed class ProbeClient : ClientBase
    {
        public Uri CurrentUri => Uri;
    }

    /// <summary>
    /// <see cref="ClientBase"/> subclass that forwards children to the base constructor, used to pin
    /// the cascading behaviour of <see cref="ClientBase.SetUri"/>.
    /// </summary>
    private sealed class ParentClient : ClientBase
    {
        public ParentClient(params ClientBase[] children)
            : base(children) { }

        public Uri CurrentUri => Uri;
    }
}
