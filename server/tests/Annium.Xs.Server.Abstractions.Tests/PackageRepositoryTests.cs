using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using NodaTime;
using Npgsql;
using Xunit;

namespace Annium.Xs.Server.Abstractions.Tests;

/// <summary>
/// Postgres-backed integration tests for the internal <c>PackageRepository&lt;TPackage,TPackageDependency&gt;</c>,
/// pinning its case-sensitivity, ordering, eager-loading, aggregation, and mutation behaviour against a real
/// <c>dotnet.packages</c> / <c>dotnet.package_dependencies</c> schema (see <see cref="PostgresFixture"/>).
/// </summary>
public class PackageRepositoryTests : TestBase, IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public PackageRepositoryTests(PostgresFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _fixture = fixture;
    }

    #region FindAllByNameAsync

    [Fact]
    public async Task FindAllByNameAsync_NameDiffersInCase_MatchesCaseInsensitively()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0"));

        // act
        var found = await repo.FindAllByNameAsync(name.ToUpperInvariant());

        // assert
        found.Has(1);
        found.At(0).Name.Is(name);
    }

    [Fact]
    public async Task FindAllByNameAsync_MultipleVersions_OrdersByVersionDescending()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0"));
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "3.0.0"));
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "2.0.0"));

        // act
        var found = await repo.FindAllByNameAsync(name);

        // assert
        found.Has(3);
        found.At(0).Version.Is("3.0.0");
        found.At(1).Version.Is("2.0.0");
        found.At(2).Version.Is("1.0.0");
    }

    [Fact]
    public async Task FindAllByNameAsync_VersionsAreNotSemverPadded_OrdersLexicographically()
    {
        // arrange — SUSPECTED DEFECT: ordering is a plain string OrderByDescending over Version, not a
        // semver-aware comparison, so "10.0.0" sorts between "1.0.0" and "2.0.0" instead of after "2.0.0".
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0"));
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "2.0.0"));
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "10.0.0"));

        // act
        var found = await repo.FindAllByNameAsync(name);

        // assert — current (lexicographic) behaviour, not semver-correct behaviour
        found.Has(3);
        found.At(0).Version.Is("2.0.0");
        found.At(1).Version.Is("10.0.0");
        found.At(2).Version.Is("1.0.0");
    }

    [Fact]
    public async Task FindAllByNameAsync_NoMatchingPackages_ReturnsEmpty()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);

        // act
        var found = await repo.FindAllByNameAsync(UniqueName("missing"));

        // assert
        found.IsEmpty();
    }

    #endregion

    #region TryFindByNameVersionAsync

    [Fact]
    public async Task TryFindByNameVersionAsync_PackageHasDependencies_EagerLoadsDependencies()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        var package = CreatePackage(metaPackageId, name, "1.0.0");
        package.Dependencies = new[]
        {
            new RepoPackageDependency
            {
                PackageId = package.Id,
                Framework = "net10.0",
                Name = "dep-a",
                Version = "1.2.3",
            },
            new RepoPackageDependency
            {
                PackageId = package.Id,
                Framework = "net10.0",
                Name = "dep-b",
                Version = "4.5.6",
            },
        };
        await repo.CreateAsync(package);

        // act
        var found = await repo.TryFindByNameVersionAsync(name, "1.0.0");

        // assert
        found.IsNotNull();
        found!.Dependencies.Has(2);
        found.Dependencies.Any(d => d.Name == "dep-a" && d.Version == "1.2.3").IsTrue();
        found.Dependencies.Any(d => d.Name == "dep-b" && d.Version == "4.5.6").IsTrue();
    }

    [Fact]
    public async Task TryFindByNameVersionAsync_NameDiffersInCase_MatchesCaseInsensitively()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0"));

        // act
        var found = await repo.TryFindByNameVersionAsync(name.ToUpperInvariant(), "1.0.0");

        // assert
        found.IsNotNull();
        found!.Version.Is("1.0.0");
    }

    [Fact]
    public async Task TryFindByNameVersionAsync_VersionAbsent_ReturnsNull()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0"));

        // act
        var found = await repo.TryFindByNameVersionAsync(name, "2.0.0");

        // assert
        found.IsNull();
    }

    [Fact]
    public async Task TryFindByNameVersionAsync_NameAbsent_ReturnsNull()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);

        // act
        var found = await repo.TryFindByNameVersionAsync(UniqueName("missing"), "1.0.0");

        // assert
        found.IsNull();
    }

    #endregion

    #region CountAllDownloadsAsync

    [Fact]
    public async Task CountAllDownloadsAsync_MultipleVersions_SumsDownloadsAcrossVersions()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0", downloads: 3));
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "2.0.0", downloads: 5));

        // act
        var total = await repo.CountAllDownloadsAsync(name);

        // assert
        total.Is(8);
    }

    [Fact]
    public async Task CountAllDownloadsAsync_NameDiffersInCase_SUSPECTED_DEFECT_ExcludesFromSum()
    {
        // arrange — SUSPECTED DEFECT: unlike FindAllByNameAsync/TryFindByNameVersionAsync/
        // DeleteByNameVersionAsync (all of which compare Name.ToUpper()), CountAllDownloadsAsync compares
        // Name directly, so a differently-cased row for the "same" package is silently excluded from the sum.
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0", downloads: 3));

        // act — querying with a differently-cased name than what was stored
        var total = await repo.CountAllDownloadsAsync(name.ToUpperInvariant());

        // assert — current (exact-case) behaviour: the differently-cased query matches nothing
        total.Is(0);
    }

    [Fact]
    public async Task CountAllDownloadsAsync_NoPackages_ReturnsZero()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);

        // act
        var total = await repo.CountAllDownloadsAsync(UniqueName("missing"));

        // assert
        total.Is(0);
    }

    #endregion

    #region IncrementDownloadsAsync

    [Fact]
    public async Task IncrementDownloadsAsync_TargetsOnlyTheGivenVersion_LeavesOtherVersionsUntouched()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        var target = CreatePackage(metaPackageId, name, "1.0.0", downloads: 4);
        var other = CreatePackage(metaPackageId, name, "2.0.0", downloads: 9);
        await repo.CreateAsync(target);
        await repo.CreateAsync(other);

        // act
        await repo.IncrementDownloadsAsync(target.Id);

        // assert
        var reloadedTarget = await repo.TryFindByNameVersionAsync(name, "1.0.0");
        var reloadedOther = await repo.TryFindByNameVersionAsync(name, "2.0.0");
        reloadedTarget!.Downloads.Is(5);
        reloadedOther!.Downloads.Is(9);
    }

    [Fact]
    public async Task IncrementDownloadsAsync_CalledTwice_IncrementsTwice()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        var package = CreatePackage(metaPackageId, name, "1.0.0", downloads: 0);
        await repo.CreateAsync(package);

        // act
        await repo.IncrementDownloadsAsync(package.Id);
        await repo.IncrementDownloadsAsync(package.Id);

        // assert
        var reloaded = await repo.TryFindByNameVersionAsync(name, "1.0.0");
        reloaded!.Downloads.Is(2);
    }

    #endregion

    #region DeleteByNameVersionAsync

    [Fact]
    public async Task DeleteByNameVersionAsync_MatchingRow_DeletesOnlyThatVersion()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0"));
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "2.0.0"));

        // act
        await repo.DeleteByNameVersionAsync(name, "1.0.0");

        // assert
        var remaining = await repo.FindAllByNameAsync(name);
        remaining.Has(1);
        remaining.At(0).Version.Is("2.0.0");
    }

    [Fact]
    public async Task DeleteByNameVersionAsync_NameDiffersInCase_MatchesCaseInsensitively()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0"));

        // act
        await repo.DeleteByNameVersionAsync(name.ToUpperInvariant(), "1.0.0");

        // assert
        var remaining = await repo.FindAllByNameAsync(name);
        remaining.IsEmpty();
    }

    [Fact]
    public async Task DeleteByNameVersionAsync_NoMatchingRow_IsNoOp()
    {
        // arrange
        await using var repo = _fixture.CreateRepository(Logger);
        var name = UniqueName("pkg");
        var metaPackageId = await SeedMetaPackageAsync(name);
        await repo.CreateAsync(CreatePackage(metaPackageId, name, "1.0.0"));

        // act
        await repo.DeleteByNameVersionAsync(name, "9.9.9");

        // assert
        var remaining = await repo.FindAllByNameAsync(name);
        remaining.Has(1);
    }

    #endregion

    private static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static RepoPackage CreatePackage(Guid metaPackageId, string name, string version, int downloads = 0) =>
        new()
        {
            Id = Guid.NewGuid(),
            MetaPackageId = metaPackageId,
            Downloads = downloads,
            Dependencies = Array.Empty<RepoPackageDependency>(),
            Name = name,
            Version = version,
            Description = "description",
            Published = Instant.FromUtc(2024, 1, 1, 0, 0),
        };

    /// <summary>
    /// Inserts a fresh <c>main.users</c> row and a <c>main.meta_packages</c> row owned by it, satisfying the
    /// foreign keys that <c>dotnet.packages.meta_package_id</c> requires, via plain SQL (the repository under
    /// test never touches these tables itself).
    /// </summary>
    private async Task<Guid> SeedMetaPackageAsync(string suffix)
    {
        var userId = Guid.NewGuid();
        var metaPackageId = Guid.NewGuid();

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using (var userCommand = connection.CreateCommand())
        {
            userCommand.CommandText =
                "insert into main.users (id, login, password_hash, api_token) values (@id, @login, 'hash', @token)";
            userCommand.Parameters.AddWithValue("id", userId);
            userCommand.Parameters.AddWithValue("login", $"user-{suffix}");
            userCommand.Parameters.AddWithValue("token", Guid.NewGuid());
            await userCommand.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }

        await using (var metaCommand = connection.CreateCommand())
        {
            metaCommand.CommandText = """
                insert into main.meta_packages (id, type, name, version, description, published, downloads, owner_id)
                values (@id, 'dotnet', @name, '0.0.0', '', now(), 0, @ownerId)
                """;
            metaCommand.Parameters.AddWithValue("id", metaPackageId);
            metaCommand.Parameters.AddWithValue("name", $"meta-{suffix}");
            metaCommand.Parameters.AddWithValue("ownerId", userId);
            await metaCommand.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }

        return metaPackageId;
    }
}
