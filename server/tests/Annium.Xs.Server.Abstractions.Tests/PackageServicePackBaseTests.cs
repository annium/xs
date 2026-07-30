using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Testing;
using Annium.Xs.Server.Abstractions.Internal.Db.Repositories;
using Annium.Xs.Server.Abstractions.Services;
using Annium.Xs.Server.Abstractions.Tools;
using Xunit;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// Tests for <see cref="PackageServicePackBase{TPackage,TPackageDependency,TPackageRequest}.RegisterAsync"/>,
/// pinning its DI wiring — repository/service registration lifetimes, the keyed <c>IUrlTool</c> resolved
/// per <c>ProjectType</c>, and that both ecosystem-specific abstract hooks are actually invoked — via
/// <see cref="TestPackageServicePack"/>, a concrete subclass closed over the <c>Test*</c> stand-ins.
/// </summary>
public class PackageServicePackBaseTests : TestBase
{
    public PackageServicePackBaseTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task RegisterAsync_RegistersRepositoryAndServiceAsScoped()
    {
        // arrange
        var container = CreateContainer();
        var pack = new TestPackageServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert
        var repositoryDescriptor = container.Single(d =>
            d.ServiceType == typeof(IPackageRepository<TestPackage, TestPackageDependency>)
        );
        repositoryDescriptor.Lifetime.Is(ServiceLifetime.Scoped);

        var serviceDescriptor = container.Single(d =>
            d.ServiceType == typeof(IPackageService<TestPackage, TestPackageDependency, TestPackageRequest>)
        );
        serviceDescriptor.Lifetime.Is(ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task RegisterAsync_InvokesBothEcosystemHooks()
    {
        // arrange
        var container = CreateContainer();
        var pack = new TestPackageServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert
        pack.RequestParserRegistered.IsTrue();
        pack.PackageStorageRegistered.IsTrue();
    }

    [Fact]
    public async Task RegisterAsync_KeyedUrlToolResolvesFromSharedConfigurationServerForProjectType()
    {
        // arrange — the UrlTool factory resolves Shared.Configuration lazily at resolve time (not at
        // registration time), so the keyed tool for ProjectType only resolves once a matching Servers
        // entry is registered.
        var container = CreateContainer();
        var serverUri = new Uri("http://example.com/api/");
        var configuration = new Shared.Configuration { Servers = { [PackageServiceFixture.ProjectType] = serverUri } };
        container.Add(configuration).AsSelf().Singleton();
        var pack = new TestPackageServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert
        await using var provider = container.BuildServiceProvider();
        var urlTool = provider.ResolveKeyed<IUrlTool>(PackageServiceFixture.ProjectType);
        urlTool.AbsoluteUrl("v1/packages").ToString().Is(new Uri(serverUri, "v1/packages").ToString());
    }

    /// <summary>
    /// A fresh container with <c>AddRuntime</c> already applied — required by
    /// <c>AddPostgreSql</c> (invoked internally by <c>RegisterAsync</c>), which resolves
    /// <c>ITypeManager</c> while wiring up entity configurations.
    /// </summary>
    private static ServiceContainer CreateContainer()
    {
        var container = new ServiceContainer();
        container.AddRuntime(typeof(PackageServicePackBaseTests).Assembly);

        return container;
    }
}
