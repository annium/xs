using Annium.Testing;
using Annium.Xs.Server.Client;
using Annium.Xs.Server.Client.Clients;
using Xunit;

namespace Annium.Xs.Server.Client.Tests;

/// <summary>
/// Runs <see cref="ServicePack.RegisterAsync"/> against a real container and pins the lifetimes it
/// declares: factories are Singleton, clients are Transient.
/// </summary>
public class ServicePackTests : ClientTestBase
{
    public ServicePackTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterServicePack<ServicePack>();
    }

    [Fact]
    public void RegisterAsync_MainClientFactory_IsSingleton()
    {
        // act
        var first = Get<MainClientFactory>();
        var second = Get<MainClientFactory>();

        // assert
        first.Is(second);
    }

    [Fact]
    public void RegisterAsync_ServerClientFactory_IsSingleton()
    {
        // act
        var first = Get<ServerClientFactory>();
        var second = Get<ServerClientFactory>();

        // assert
        first.Is(second);
    }

    [Fact]
    public void RegisterAsync_MainClient_IsTransient()
    {
        // act
        var first = Get<MainClient>();
        var second = Get<MainClient>();

        // assert
        first.IsNot(second);
    }

    [Fact]
    public void RegisterAsync_ServerClient_IsTransient()
    {
        // act
        var first = Get<ServerClient>();
        var second = Get<ServerClient>();

        // assert
        first.IsNot(second);
    }
}
