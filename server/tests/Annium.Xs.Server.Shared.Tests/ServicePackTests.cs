using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Testing;
using Annium.Xs.Server.Shared.Auth.TokenAccessors;
using Annium.Xs.Server.Shared.Internal.Auth;
using Annium.Xs.Server.Shared.Internal.Repositories;
using Annium.Xs.Server.Shared.Internal.Tools;
using Annium.Xs.Server.Shared.Repositories;
using Annium.Xs.Server.Shared.Tools;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="ServicePack.RegisterAsync"/>, pinning every registration it declares — concrete
/// implementation and lifetime — without ever calling <see cref="ServicePack.SetupAsync"/> (which needs a
/// live database and is already covered, via the real migrations, by the smoke test in
/// <c>SchemaTests.Migrations_FreshDatabase_AppliedCleanlyAndLeftExpectedSeedRow</c>).
/// </summary>
/// <remarks>
/// The repository registrations (<see cref="IMetaPackageRepository"/>, <see cref="IUserRepository"/>) are
/// discovered by <c>RegisterAsync</c>'s reflection convention —
/// <c>AddAll(...).Where(x =&gt; ...Name.EndsWith("Repository"))</c> — which is exactly what makes it risky:
/// a repository renamed, or a new one added, without the "Repository" suffix registers nothing here and
/// only fails at runtime resolution. Actually resolving <see cref="IMetaPackageRepository"/>/
/// <see cref="IUserRepository"/> end-to-end would need a live Postgres connection string, since
/// <c>RegisterAsync</c>'s <c>container.AddPostgreSql&lt;Connection&gt;()</c> wires each repository to a real
/// <c>Connection</c> (and, transitively, a JSON serializer registration to build linq2db's mapping schema) —
/// none of which <c>RegisterAsync</c> itself needs. So these two are pinned via their registration
/// descriptors (service type, concrete implementation type, lifetime) instead — matching the same
/// approach <c>PackageServicePackBaseTests.RegisterAsync_RegistersRepositoryAndServiceAsScoped</c> uses in
/// <c>Annium.Xs.Server.Abstractions.Tests</c>, for the same reason.
/// </remarks>
public class ServicePackTests : TestBase
{
    public ServicePackTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    #region repositories

    [Fact]
    public async Task RegisterAsync_RegistersMetaPackageRepositoryAsScopedIMetaPackageRepository()
    {
        // arrange
        var container = CreateContainer();
        var pack = new ServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert — the interface is registered as a factory delegating to the self-registered concrete
        // type (that's how the bulk-registration builder wires AsInterfaces(); see In()'s doc comment on
        // BulkRegistrationBuilder), so pinning "IMetaPackageRepository resolves to MetaPackageRepository"
        // means checking both: the interface descriptor's own lifetime, and that the concrete type the
        // reflection convention discovered and self-registered is exactly MetaPackageRepository.
        var interfaceDescriptor = container.Single(d => d.ServiceType == typeof(IMetaPackageRepository));
        interfaceDescriptor.Lifetime.Is(ServiceLifetime.Scoped);

        var selfDescriptor = (ITypeServiceDescriptor)
            container.Single(d => d.ServiceType == typeof(MetaPackageRepository));
        selfDescriptor.ImplementationType.Is(typeof(MetaPackageRepository));
        selfDescriptor.Lifetime.Is(ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task RegisterAsync_RegistersUserRepositoryAsScopedIUserRepository()
    {
        // arrange
        var container = CreateContainer();
        var pack = new ServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert — see the mirrored comment in RegisterAsync_RegistersMetaPackageRepositoryAsScopedIMetaPackageRepository.
        var interfaceDescriptor = container.Single(d => d.ServiceType == typeof(IUserRepository));
        interfaceDescriptor.Lifetime.Is(ServiceLifetime.Scoped);

        var selfDescriptor = (ITypeServiceDescriptor)container.Single(d => d.ServiceType == typeof(UserRepository));
        selfDescriptor.ImplementationType.Is(typeof(UserRepository));
        selfDescriptor.Lifetime.Is(ServiceLifetime.Scoped);
    }

    #endregion

    #region tools

    [Fact]
    public async Task RegisterAsync_MetaPackageTool_ResolvesAsSingletonIMetaPackageTool()
    {
        // arrange
        var container = CreateContainer();
        var pack = new ServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert
        await using var provider = container.BuildServiceProvider();
        var first = provider.Resolve<IMetaPackageTool>();
        var second = provider.Resolve<IMetaPackageTool>();

        first.GetType().Is(typeof(MetaPackageTool));
        ReferenceEquals(first, second).IsTrue();
    }

    #endregion

    #region auth

    [Fact]
    public async Task RegisterAsync_AuthorizationFilter_ResolvesAsSelfSingleton()
    {
        // arrange
        var container = CreateContainer();
        var pack = new ServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert
        await using var provider = container.BuildServiceProvider();
        var first = provider.Resolve<AuthorizationFilter>();
        var second = provider.Resolve<AuthorizationFilter>();

        ReferenceEquals(first, second).IsTrue();
    }

    [Fact]
    public async Task RegisterAsync_BearerTokenAccessor_ResolvesAsSingletonITokenAccessor()
    {
        // arrange — the bearer accessor is registered here, once, rather than re-constructed by each
        // ecosystem's ServicePack. AuthorizationFilter resolves IEnumerable<ITokenAccessor>, so what
        // matters is that it is reachable through the ITokenAccessor service type (not only as its own
        // concrete type) — an ecosystem pack adding a further accessor appends to that same enumerable.
        var container = CreateContainer();
        var pack = new ServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert
        await using var provider = container.BuildServiceProvider();
        var accessors = provider.Resolve<IEnumerable<ITokenAccessor>>().ToArray();

        accessors.Has(1);
        accessors[0].GetType().Is(typeof(BearerTokenAccessor));

        // singleton: the same instance is handed out on every resolve
        var second = provider.Resolve<IEnumerable<ITokenAccessor>>().Single();
        ReferenceEquals(accessors[0], second).IsTrue();
    }

    [Fact]
    public async Task RegisterAsync_AuthorizationApplicationModelProvider_ResolvesAsSingletonIApplicationModelProvider()
    {
        // arrange
        var container = CreateContainer();
        var pack = new ServicePack();

        // act
        await using var bootstrapProvider = container.BuildServiceProvider();
        await pack.RegisterAsync(container, bootstrapProvider, TestContext.Current.CancellationToken);

        // assert
        await using var provider = container.BuildServiceProvider();
        var first = provider.Resolve<IApplicationModelProvider>();
        var second = provider.Resolve<IApplicationModelProvider>();

        first.GetType().Is(typeof(AuthorizationApplicationModelProvider));
        ReferenceEquals(first, second).IsTrue();
    }

    #endregion

    /// <summary>
    /// A fresh container with <c>AddRuntime</c> already applied — required by <c>AddPostgreSql</c>
    /// (invoked internally by <c>RegisterAsync</c>), which resolves <c>ITypeManager</c> while wiring up
    /// entity configurations.
    /// </summary>
    private static ServiceContainer CreateContainer()
    {
        var container = new ServiceContainer();
        container.AddRuntime(typeof(ServicePackTests).Assembly);

        return container;
    }
}
