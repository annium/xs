using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.DbUp.Core;
using Annium.DbUp.PostgreSql;
using Annium.linq2db.Extensions;
using Annium.Logging;
using Annium.Xs.Server.Abstractions.Internal.Db;
using Annium.Xs.Server.Abstractions.Internal.Db.Repositories;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Mapping;
using NodaTime;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// Concrete, DB-mappable stand-in for the generic <c>TPackageDependency</c> parameter of
/// <see cref="PackageRepository{TPackage,TPackageDependency}"/>. Mapped onto the real
/// <c>dotnet.package_dependencies</c> table (see <see cref="PostgresFixture"/>).
/// </summary>
internal sealed class RepoPackageDependency : IPackageDependency
{
    public Guid PackageId { get; set; }
    public string Framework { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Concrete, DB-mappable stand-in for the generic <c>TPackage</c> parameter of
/// <see cref="PackageRepository{TPackage,TPackageDependency}"/>. Mapped onto the real
/// <c>dotnet.packages</c> table (see <see cref="PostgresFixture"/>).
/// </summary>
internal sealed class RepoPackage : IPackage<RepoPackageDependency>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MetaPackageId { get; set; }
    public int Downloads { get; set; }
    public IReadOnlyCollection<RepoPackageDependency> Dependencies { get; set; } = Array.Empty<RepoPackageDependency>();
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Instant Published { get; set; }
}

/// <summary>
/// xunit.v3 class fixture that starts a single <c>postgres:17-alpine</c> Testcontainers instance for the
/// whole <see cref="PackageRepositoryTests"/> class, initializes the real schema against it, and hands out
/// fresh <see cref="PackageRepository{TPackage,TPackageDependency}"/> instances (one per pooled connection)
/// closed over <see cref="RepoPackage"/>/<see cref="RepoPackageDependency"/>.
/// </summary>
/// <remarks>
/// Schema setup is split in two:
/// <list type="bullet">
/// <item>
/// The <c>main</c> schema (<c>users</c>, <c>meta_packages</c>, <c>meta_package_permissions</c> + seed) is
/// created by running the real DbUp migrations embedded in <c>Annium.Xs.Server.Shared</c> — the same
/// scripts <c>Annium.Xs.Server.Shared.ServicePack.SetupAsync</c> runs in production.
/// </item>
/// <item>
/// The <c>dotnet</c> schema (<c>packages</c>, <c>package_dependencies</c>) — the tables
/// <see cref="PackageRepository{TPackage,TPackageDependency}"/> actually targets — is defined by DbUp
/// migrations embedded in <c>Annium.Xs.Server.Dotnet</c>, an assembly this test project has no reference to
/// (out of scope for this task's file ownership). Those two tables are therefore hand-created here, with DDL
/// copied verbatim from <c>server/src/Annium.Xs.Server.Dotnet/Scripts/Migrations/0001_packages.sql</c> and
/// <c>0002_package_dependencies.sql</c>.
/// </item>
/// </list>
/// </remarks>
public sealed class PostgresFixture : IAsyncLifetime, IAsyncDisposable
{
    private const string DotnetSchemaSql = """
        create schema if not exists dotnet;

        create table dotnet.packages (
            id uuid not null,
            meta_package_id uuid not null,
            name text not null,
            version text not null,
            description text not null,
            published timestamptz not null,
            downloads int not null,
            constraint pk_packages primary key (id),
            constraint fk_packages_meta_packages_meta_package_id foreign key (meta_package_id) references main.meta_packages(id) on delete cascade
        );
        create unique index ix_packages_name_version on dotnet.packages using btree (name, version);

        create table dotnet.package_dependencies (
            package_id uuid not null,
            framework text not null,
            name text not null,
            version text not null,
            constraint pk_package_dependencies primary key (package_id, framework, name, version),
            constraint fk_package_dependencies_packages_package_id foreign key (package_id) references dotnet.packages(id) on delete cascade
        );
        """;

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("xs_package_repository_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly MappingSchema _mappingSchema = BuildMappingSchema();

    private NpgsqlDataSource? _dataSource;

    /// <summary>
    /// The live connection string for the started container. Only valid after <see cref="InitializeAsync"/>.
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        Migrator
            .Instance.ForPostgresql(ConnectionString, "main")
            .WithScriptsFromAssembly(typeof(Shared.ServicePack).Assembly)
            .Execute();

        await using (var connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = DotnetSchemaSql;
            await command.ExecuteNonQueryAsync();
        }

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
        dataSourceBuilder.UseNodaTime();
        _dataSource = dataSourceBuilder.Build();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (_dataSource is not null)
            await _dataSource.DisposeAsync();

        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a fresh repository backed by a new pooled connection. Disposing the returned repository
    /// (it is <see cref="IAsyncDisposable"/>) disposes the underlying connection.
    /// </summary>
    internal PackageRepository<RepoPackage, RepoPackageDependency> CreateRepository(ILogger logger)
    {
        if (_dataSource is null)
            throw new InvalidOperationException($"{nameof(InitializeAsync)} has not completed yet.");

        var connection = _dataSource.CreateConnection();
        var options = new DataOptions()
            .UseConnection(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v15), connection, true)
            .UseMappingSchema(_mappingSchema);
        var dataOptions = new DataOptions<ServerConnection<RepoPackage, RepoPackageDependency>>(options);
        var db = new ServerConnection<RepoPackage, RepoPackageDependency>(dataOptions, logger);

        return new PackageRepository<RepoPackage, RepoPackageDependency>(db);
    }

    /// <summary>
    /// Builds the mapping schema for <see cref="RepoPackage"/>/<see cref="RepoPackageDependency"/> by hand,
    /// mirroring the real (internal, out-of-scope) <c>PackageConfiguration</c>/<c>PackageDependencyConfiguration</c>
    /// fluent mappings in <c>Annium.Xs.Server.Dotnet</c>.
    /// </summary>
    private static MappingSchema BuildMappingSchema()
    {
        var mappingSchema = new MappingSchema();
        var mappingBuilder = new FluentMappingBuilder(mappingSchema);

        var packageBuilder = mappingBuilder.Entity<RepoPackage>();
        packageBuilder.HasSchemaName("dotnet").HasTableName("packages");
        packageBuilder.HasPrimaryKey(x => x.Id);
        packageBuilder.Property(x => x.Id).IsColumn();
        packageBuilder.Property(x => x.MetaPackageId).IsColumn();
        packageBuilder.Property(x => x.Name).IsColumn();
        packageBuilder.Property(x => x.Version).IsColumn();
        packageBuilder.Property(x => x.Description).IsColumn();
        packageBuilder.Property(x => x.Published).IsColumn();
        packageBuilder.Property(x => x.Downloads).IsColumn();
        packageBuilder.Property(x => x.Dependencies).IsNotColumn();
        packageBuilder.Association(x => x.Dependencies, x => x.Id, x => x.PackageId, canBeNull: false);

        var dependencyBuilder = mappingBuilder.Entity<RepoPackageDependency>();
        dependencyBuilder.HasSchemaName("dotnet").HasTableName("package_dependencies");
        dependencyBuilder.Property(x => x.PackageId).IsColumn();
        dependencyBuilder.Property(x => x.Framework).IsColumn();
        dependencyBuilder.Property(x => x.Name).IsColumn();
        dependencyBuilder.Property(x => x.Version).IsColumn();

        mappingBuilder.Build();
        mappingSchema.UseSnakeCaseColumns();

        return mappingSchema;
    }
}
