using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.DbUp.Core;
using Annium.DbUp.PostgreSql;
using Annium.linq2db.Extensions;
using Annium.Logging;
using Annium.Xs.Server.Shared.Internal;
using Annium.Xs.Server.Shared.Internal.Configurations;
using Annium.Xs.Server.Shared.Internal.Repositories;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Mapping;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// xunit.v3 collection fixture that starts a single <c>postgres:17-alpine</c> Testcontainers instance,
/// shared by every DB-backed test class in this collection, and hands out fresh <see cref="Connection"/>
/// / repository instances (one per pooled connection) closed over the real, internal
/// <c>Annium.Xs.Server.Shared</c> mapping.
/// </summary>
/// <remarks>
/// Unlike <c>Annium.Xs.Server.Abstractions.Tests.PostgresFixture</c>, no hand-copied DDL is needed here:
/// <c>Annium.Xs.Server.Shared</c> ships its own DbUp migrations as embedded resources, so the real
/// <c>main</c> schema (<c>users</c>, <c>meta_packages</c>, <c>meta_package_permissions</c> + seed) is
/// produced by running those scripts directly — the same ones <c>ServicePack.SetupAsync</c> runs in
/// production. The mapping schema is built the same way production does: the real (internal, reachable
/// via <c>InternalsVisibleTo</c>) <see cref="UserConfiguration"/>/<see cref="MetaPackageConfiguration"/>/
/// <see cref="MetaPackagePermissionConfiguration"/> classes are fed through the same
/// <see cref="MappingSchemaExtensionsBase.ApplyConfigurations"/> pipeline <c>AddPostgreSql&lt;TConnection&gt;</c>
/// uses, so association-derived FK columns (e.g. <c>MetaPackage.OwnerId</c>, only ever declared via the
/// <c>Owner</c> association) end up mapped identically to production.
/// </remarks>
public sealed class PostgresFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("xs_server_shared_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly MappingSchema _mappingSchema = BuildMappingSchema();

    private NpgsqlDataSource? _dataSource;

    /// <summary>
    /// The live connection string for the started container. Only valid after <see cref="InitializeAsync"/>.
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        Migrator
            .Instance.ForPostgresql(ConnectionString, Constants.Schema)
            .WithScriptsFromAssembly(typeof(ServicePack).Assembly)
            .Execute();

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
        dataSourceBuilder.UseNodaTime();
        _dataSource = dataSourceBuilder.Build();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (_dataSource is not null)
            await _dataSource.DisposeAsync();

        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a fresh, real <see cref="Connection"/> backed by a new pooled connection, using the
    /// production mapping schema.
    /// </summary>
    /// <param name="logger">The logger to attach to the connection.</param>
    internal Connection CreateConnection(ILogger logger)
    {
        if (_dataSource is null)
            throw new InvalidOperationException($"{nameof(InitializeAsync)} has not completed yet.");

        var connection = _dataSource.CreateConnection();
        var options = new DataOptions()
            .UseConnection(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v15), connection, true)
            .UseMappingSchema(_mappingSchema);
        var dataOptions = new DataOptions<Connection>(options);

        return new Connection(dataOptions, logger);
    }

    /// <summary>
    /// Creates a fresh <see cref="MetaPackageRepository"/> backed by a new pooled connection. Disposing
    /// the returned repository (it is <see cref="IAsyncDisposable"/>) disposes the underlying connection.
    /// </summary>
    /// <param name="logger">The logger to attach to the underlying connection.</param>
    internal MetaPackageRepository CreateMetaPackageRepository(ILogger logger) => new(CreateConnection(logger));

    /// <summary>
    /// Creates a fresh <see cref="UserRepository"/> backed by a new pooled connection. Disposing the
    /// returned repository (it is <see cref="IAsyncDisposable"/>) disposes the underlying connection.
    /// </summary>
    /// <param name="logger">The logger to attach to the underlying connection.</param>
    internal UserRepository CreateUserRepository(ILogger logger) => new(CreateConnection(logger));

    /// <summary>
    /// Builds the mapping schema by driving the real, internal entity configuration classes through the
    /// same <c>ApplyConfigurations</c>/<c>UseSnakeCaseColumns</c> pipeline production uses (see
    /// <c>Annium.linq2db.PostgreSql.ServiceContainerExtensions.AddPostgreSql</c>), instead of hand-mirroring
    /// the fluent mappings.
    /// </summary>
    private static MappingSchema BuildMappingSchema()
    {
        var container = new ServiceContainer();
        container.Add(new UserConfiguration()).AsInterfaces().Singleton();
        container.Add(new MetaPackageConfiguration()).AsInterfaces().Singleton();
        container.Add(new MetaPackagePermissionConfiguration()).AsInterfaces().Singleton();
        using var provider = container.BuildServiceProvider();

        var mappingSchema = new MappingSchema();
        mappingSchema.ApplyConfigurations(provider).UseSnakeCaseColumns();

        return mappingSchema;
    }
}

/// <summary>
/// Collection definition tying every DB-backed <c>Annium.Xs.Server.Shared.Tests</c> class to a single
/// shared <see cref="PostgresFixture"/> instance, so the (slow-to-start) Postgres container is started
/// once for the whole collection rather than once per test class.
/// </summary>
[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    /// <summary>
    /// The collection name referenced by <c>[Collection(PostgresCollection.Name)]</c> on every DB-backed
    /// test class.
    /// </summary>
    public const string Name = "Postgres";
}
