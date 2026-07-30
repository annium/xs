using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Shared.Domain.Enums;
using Annium.Xs.Server.Shared.Domain.Models;
using Npgsql;
using Xunit;
using static Annium.Xs.Server.Shared.Tests.Helper;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Postgres-backed tests pinning behaviour the real DbUp-applied schema enforces (constraints, cascades)
/// rather than the migration SQL itself — plus a smoke test that the real migrations apply cleanly to a
/// fresh database and leave the expected seed row.
/// </summary>
[Collection(PostgresCollection.Name)]
public class SchemaTests : TestBase
{
    private readonly PostgresFixture _fixture;

    public SchemaTests(PostgresFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _fixture = fixture;
    }

    #region main.users unique login index

    [Fact]
    public async Task Users_DuplicateLogin_ThrowsPostgresException()
    {
        // arrange — main.users has a unique index on `login` (Scripts/Migrations/0001_users.sql:8).
        await using var repo = _fixture.CreateUserRepository(Logger);
        var login = UniqueName("dup-login");
        await repo.CreateAsync(new User(login, "hash", Guid.NewGuid()));

        // act & assert
        await Wrap.It(async () => await repo.CreateAsync(new User(login, "hash", Guid.NewGuid())))
            .ThrowsAsync<PostgresException>();
    }

    #endregion

    #region main.meta_packages unique (type, name, version) index

    [Fact]
    public async Task MetaPackages_DuplicateTypeNameVersion_ThrowsPostgresException()
    {
        // arrange — main.meta_packages has a unique index on (type, name, version)
        // (Scripts/Migrations/0002_meta_packages.sql:13).
        await using var userRepo = _fixture.CreateUserRepository(Logger);
        await using var packageRepo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = new User(UniqueName("owner"), "hash", Guid.NewGuid());
        await userRepo.CreateAsync(owner);
        var type = ProjectType.Register(UniqueName("type"));
        var name = UniqueName("package");
        await CreatePackageAsync(packageRepo, owner, [], type: type, name: name, version: "1.0.0");

        // act & assert
        var exception = await Wrap.It(async () =>
                await CreatePackageAsync(packageRepo, owner, [], type: type, name: name, version: "1.0.0")
            )
            .ThrowsAsync<PostgresException>();
        exception.SqlState.Is("23505");
    }

    #endregion

    #region FK cascades

    [Fact]
    public async Task Users_DeletingUser_CascadesToOwnedMetaPackages()
    {
        // arrange — fk_meta_packages_users_owner_id is declared "on delete cascade"
        // (Scripts/Migrations/0002_meta_packages.sql:11).
        await using var userRepo = _fixture.CreateUserRepository(Logger);
        await using var packageRepo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = new User(UniqueName("owner"), "hash", Guid.NewGuid());
        await userRepo.CreateAsync(owner);
        var package = await CreatePackageAsync(packageRepo, owner, []);

        // act
        await userRepo.DeleteByIdAsync(owner.Id);

        // assert — the owned package was cascade-deleted along with its owner, not left orphaned
        var reloaded = await packageRepo.TryGetByIdAsync(package.Id);
        reloaded.IsNull();
    }

    [Fact]
    public async Task MetaPackages_DeletingMetaPackage_CascadesToItsPermissions()
    {
        // arrange — fk_meta_package_permissions_meta_packages_meta_package_id is declared
        // "on delete cascade" (Scripts/Migrations/0003_meta_package_permissions.sql:6). Deleting via raw
        // SQL (rather than MetaPackageRepository.DeleteByIdAsync, which itself only issues a DELETE
        // against meta_packages) isolates the assertion to the database's own cascade behaviour.
        await using var userRepo = _fixture.CreateUserRepository(Logger);
        await using var packageRepo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = new User(UniqueName("owner"), "hash", Guid.NewGuid());
        await userRepo.CreateAsync(owner);
        var package = await CreatePackageAsync(
            packageRepo,
            owner,
            [new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Read)]
        );

        // act
        await packageRepo.DeleteByIdAsync(package.Id);

        // assert — no permission rows for the deleted package remain
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "select count(*) from main.meta_package_permissions where meta_package_id = @id";
        command.Parameters.AddWithValue("id", package.Id);
        var remaining = (long)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!;
        remaining.Is(0L);
    }

    #endregion

    #region meta_package_permissions primary key enforces one row per category

    [Fact]
    public async Task MetaPackagePermissions_SameCategoryDifferentPermissionValue_ThrowsPostgresException()
    {
        // arrange — pk_meta_package_permissions is (meta_package_id, category), narrowed by
        // Scripts/Migrations/0005_meta_package_permissions_pk.sql from the original
        // (meta_package_id, category, permission), which had included the one column meant to be mutable
        // per category and so failed to enforce the invariant. The key matters because
        // UserMetaPackageAccess resolves a user's effective permission with
        // `permissions.FirstOrDefault(p => p.Category == category)` (UserMetaPackageAccess.cs:22) —
        // duplicate rows for one category would make the authorization outcome depend on row order.
        await using var userRepo = _fixture.CreateUserRepository(Logger);
        await using var packageRepo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = new User(UniqueName("owner"), "hash", Guid.NewGuid());
        await userRepo.CreateAsync(owner);

        // act
        var exception = await Wrap.It(async () =>
                await CreatePackageAsync(
                    packageRepo,
                    owner,
                    [
                        new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Read),
                        new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Publish),
                    ]
                )
            )
            .ThrowsAsync<PostgresException>();

        // assert — 23505 is unique_violation
        exception.SqlState.Is("23505");
    }

    #endregion

    #region migration smoke

    [Fact]
    public async Task Migrations_FreshDatabase_AppliedCleanlyAndLeftExpectedSeedRow()
    {
        // arrange — no action: the fixture already ran the real DbUp scripts (Scripts/Init +
        // Scripts/Migrations, embedded in Annium.Xs.Server.Shared) against a fresh container at
        // collection start-up, exactly as ServicePack.SetupAsync does in production.
        await using var repo = _fixture.CreateUserRepository(Logger);

        // act — 0004_seed.sql seeds exactly one 'alex' user with a known id/token
        var seeded = await repo.TryFindByLoginAsync("alex");

        // assert
        seeded.IsNotNull();
        seeded!.Id.Is(Guid.Parse("baa0ad0f-91c5-4c19-963c-ea369048e67a"));
        seeded.ApiToken.Is(Guid.Parse("621ecf4e-0771-4d95-a4e5-2b49f17a1127"));
    }

    #endregion

    private static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}";
}
