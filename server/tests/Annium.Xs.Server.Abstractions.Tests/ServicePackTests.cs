using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Xs.Server.Abstractions.Internal.Services;
using Annium.Xs.Server.Abstractions.Services;
using Xunit;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// Tests for <see cref="ServicePack.RegisterAsync"/>, pinning its <see cref="IStorageFactory"/>
/// registration — the concrete implementation and its singleton lifetime.
/// </summary>
public class ServicePackTests : TestBase
{
    public ServicePackTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task RegisterAsync_RegistersFileStorageFactoryAsSingleton()
    {
        // arrange
        var container = new ServiceContainer();
        var pack = new ServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert
        await using var provider = container.BuildServiceProvider();
        var first = provider.Resolve<IStorageFactory>();
        var second = provider.Resolve<IStorageFactory>();

        first.GetType().Is(typeof(FileStorageFactory));
        ReferenceEquals(first, second).IsTrue();
    }
}
