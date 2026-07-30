using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Shared.Domain.Enums;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Internal.Repositories;
using NodaTime;
using Xunit;
using static Annium.Xs.Server.Shared.Tests.Helper;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Postgres-backed integration tests for the internal <see cref="MetaPackageRepository"/>, pinning its
/// visibility predicate (<see cref="MetaPackageRepository.FindAllAsync"/>), filtering, pagination, and CRUD
/// behaviour against the real <c>main.meta_packages</c> / <c>main.meta_package_permissions</c> schema.
/// </summary>
[Collection(PostgresCollection.Name)]
public class MetaPackageRepositoryTests : TestBase
{
    private readonly PostgresFixture _fixture;

    public MetaPackageRepositoryTests(PostgresFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _fixture = fixture;
    }

    #region FindAllAsync visibility predicate

    [Fact]
    public async Task FindAllAsync_OwnerNoExplicitPermissionRow_SeesOwnPackage()
    {
        // arrange — owner branch of the predicate (x.OwnerId == userId) needs no permission row at all.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: []);

        // act
        var found = await repo.FindAllAsync(owner.Id, null, null, 1, 1000);

        // assert
        found.Any(x => x.Id == package.Id).IsTrue();
    }

    [Fact]
    public async Task FindAllAsync_ReturnedPackages_HaveOwnerAndPermissionsEagerlyLoaded()
    {
        // arrange — FindAllAsync eager-loads both navigation properties (LoadWith(x => x.Owner) and
        // LoadWith(x => x.Permissions)). Callers read them straight off the result without a second
        // query — Annium.Xs.Server.Main/Views/Responses/MetaPackageResponse.cs does `metaPackage.Owner.Login`
        // and `metaPackage.Permissions` — so dropping either LoadWith would NRE or silently return empty
        // permissions in production. The other FindAllAsync tests only assert on Id, which would not notice.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var name = UniqueName("package");
        var package = await CreatePackageAsync(
            repo,
            owner,
            permissions:
            [
                new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Read | Permission.Publish),
                new MetaPackagePermission(Guid.Empty, PermissionCategory.World, Permission.None),
            ],
            name: name
        );

        // act — scope by name so the assertion sees exactly the package seeded here
        var found = await repo.FindAllAsync(owner.Id, null, name, 1, 1000);

        // assert
        var reloaded = found.Single(x => x.Id == package.Id);

        reloaded.Owner.IsNotNull();
        reloaded.Owner.Id.Is(owner.Id);
        reloaded.Owner.Login.Is(owner.Login);

        reloaded.Permissions.Has(2);
        reloaded
            .Permissions.Single(p => p.Category == PermissionCategory.Owner)
            .Permission.Is(Permission.Read | Permission.Publish);
        reloaded.Permissions.Single(p => p.Category == PermissionCategory.World).Permission.Is(Permission.None);
    }

    [Fact]
    public async Task FindAllAsync_NonOwnerWithOwnerCategoryReadPermissionRow_DoesNotSeePackage()
    {
        // arrange — an Owner-category grant describes what the OWNER may do; it must not make the package
        // visible to anyone else. Every package gets an Owner Read|Publish row at creation
        // (MetaPackageTool.Generate), so matching on it here would make the predicate true for
        // essentially every row in the table and defeat the visibility model entirely.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var other = await CreateUserAsync();
        var package = await CreatePackageAsync(
            repo,
            owner,
            permissions: [new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Read)]
        );

        // act — asking as `other`, a user with no relation to the package or the permission row
        var found = await repo.FindAllAsync(other.Id, null, null, 1, 1000);

        // assert
        found.Any(x => x.Id == package.Id).IsFalse();
    }

    [Fact]
    public async Task FindAllAsync_NonOwnerWithWorldCategoryReadPermissionRow_SeesPackage()
    {
        // arrange — the World-category Read grant is what makes a package publicly listable. It is set by
        // the owner through POST /{type}/{name}/permissions; packages start at World: None, i.e. private.
        // This mirrors the per-resource check the single-package endpoint already performs via
        // MetaPackageAccess.ForUser(user).Has(Permission.Read).
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var other = await CreateUserAsync();
        var package = await CreatePackageAsync(
            repo,
            owner,
            permissions: [new MetaPackagePermission(Guid.Empty, PermissionCategory.World, Permission.Read)]
        );

        // act
        var found = await repo.FindAllAsync(other.Id, null, null, 1, 1000);

        // assert
        found.Any(x => x.Id == package.Id).IsTrue();
    }

    [Fact]
    public async Task FindAllAsync_NonOwnerNoMatchingPermissionRow_DoesNotSeePackage()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var other = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: []);

        // act
        var found = await repo.FindAllAsync(other.Id, null, null, 1, 1000);

        // assert
        found.Any(x => x.Id == package.Id).IsFalse();
    }

    #endregion

    #region FindAllAsync type/query filters

    [Fact]
    public async Task FindAllAsync_TypeFilter_NarrowsToMatchingType()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var typeA = ProjectType.Register(UniqueName("type-a"));
        var typeB = ProjectType.Register(UniqueName("type-b"));
        var packageA = await CreatePackageAsync(repo, owner, permissions: [], type: typeA);
        var packageB = await CreatePackageAsync(repo, owner, permissions: [], type: typeB);

        // act
        var found = await repo.FindAllAsync(owner.Id, typeA, null, 1, 1000);

        // assert
        found.Any(x => x.Id == packageA.Id).IsTrue();
        found.Any(x => x.Id == packageB.Id).IsFalse();
    }

    [Fact]
    public async Task FindAllAsync_QueryFilter_MatchesNameCaseInsensitively()
    {
        // arrange — request.Where(x.Name.ToUpper().Contains(query.ToUpperInvariant())) upper-cases both
        // sides, so the match is case-insensitive.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var name = UniqueName("MixedCaseName");
        var package = await CreatePackageAsync(repo, owner, permissions: [], name: name);

        // act — querying with a differently-cased substring than what was stored
        var found = await repo.FindAllAsync(owner.Id, null, name.ToUpperInvariant()[..8], 1, 10);

        // assert
        found.Any(x => x.Id == package.Id).IsTrue();
    }

    [Fact]
    public async Task FindAllAsync_QueryFilter_NoMatch_ExcludesPackage()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: []);

        // act
        var found = await repo.FindAllAsync(owner.Id, null, UniqueName("no-such-substring"), 1, 10);

        // assert
        found.Any(x => x.Id == package.Id).IsFalse();
    }

    #endregion

    #region FindAllAsync pagination

    [Fact]
    public async Task FindAllAsync_Pagination_SlicesResultsAcrossPagesWithoutOverlapOrLoss()
    {
        // arrange — the query has no ORDER BY before Skip/Take (see MetaPackageRepository.cs:51-57), so
        // which specific package lands on which page is not something this test can rely on (that
        // absence of ordering is itself worth flagging — see report). What Skip/Take does guarantee
        // regardless of order is: every page-worth of rows is disjoint from every other, and every row
        // shows up on exactly one page across the full result set — so pin that instead.
        //
        // The `query` filter scopes every call down to just this test's own packages via a unique
        // shared name tag. This isn't just tidiness: the FindAllAsync_NonOwner...SUSPECTED_DEFECT test
        // above demonstrates that a package with an Owner-category Read permission row is visible to
        // *every* caller, and other test classes in this collection share this same Postgres container
        // for its whole lifetime — so without scoping, unrelated packages leaked by that defect would
        // inflate these page counts non-deterministically.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var tag = UniqueName("pagination");
        var ids = new List<Guid>();
        for (var i = 0; i < 5; i++)
            ids.Add((await CreatePackageAsync(repo, owner, permissions: [], name: $"{tag}-{i}")).Id);

        // act
        var page1 = await repo.FindAllAsync(owner.Id, null, tag, 1, 2);
        var page2 = await repo.FindAllAsync(owner.Id, null, tag, 2, 2);
        var page3 = await repo.FindAllAsync(owner.Id, null, tag, 3, 2);

        // assert
        page1.Has(2);
        page2.Has(2);
        page3.Has(1);
        var seen = page1.Concat(page2).Concat(page3).Select(x => x.Id).ToArray();
        seen.Distinct().Count().Is(seen.Length);
        ids.All(id => seen.Contains(id)).IsTrue();
    }

    [Fact]
    public async Task FindAllAsync_CountZero_ReturnsEmptyResult()
    {
        // arrange — pinning the observed behaviour of Take(0): an empty result, not an exception and
        // not "unlimited"/"default" rows.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        await CreatePackageAsync(repo, owner, permissions: []);

        // act
        var found = await repo.FindAllAsync(owner.Id, null, null, 1, 0);

        // assert
        found.IsEmpty();
    }

    [Fact]
    public async Task FindAllAsync_PageBelowOne_ThrowsArgumentOutOfRange()
    {
        // arrange — pages are 1-based. `page = 0` would make `Skip((page - 1) * count)` a negative OFFSET,
        // which Postgres rejects with an opaque 2201X; the guard turns that into a clear contract violation
        // at the call site instead of a 500 from the database.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        await CreatePackageAsync(repo, owner, permissions: []);

        // act & assert
        await Wrap.It(async () => await repo.FindAllAsync(owner.Id, null, null, 0, 10))
            .ThrowsAsync<ArgumentOutOfRangeException>();
        await Wrap.It(async () => await repo.FindAllAsync(owner.Id, null, null, -1, 10))
            .ThrowsAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task FindAllAsync_NegativeCount_ThrowsArgumentOutOfRange()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        await CreatePackageAsync(repo, owner, permissions: []);

        // act & assert
        await Wrap.It(async () => await repo.FindAllAsync(owner.Id, null, null, 1, -1))
            .ThrowsAsync<ArgumentOutOfRangeException>();
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_PersistsPackageAndItsPermissions()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var permissions = new[]
        {
            new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Read | Permission.Publish),
            new MetaPackagePermission(Guid.Empty, PermissionCategory.World, Permission.Read),
        };

        // act
        var package = await CreatePackageAsync(repo, owner, permissions);

        // assert
        var reloaded = await repo.TryGetByIdAsync(package.Id);
        reloaded.IsNotNull();
        reloaded!.Name.Is(package.Name);
        reloaded.OwnerId.Is(owner.Id);
        reloaded.Permissions.Has(2);
        reloaded
            .Permissions.Any(p =>
                p.Category == PermissionCategory.Owner && p.Permission == (Permission.Read | Permission.Publish)
            )
            .IsTrue();
        reloaded
            .Permissions.Any(p => p.Category == PermissionCategory.World && p.Permission == Permission.Read)
            .IsTrue();
    }

    #endregion

    #region TryGetByIdAsync

    [Fact]
    public async Task TryGetByIdAsync_IdAbsent_ReturnsNull()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);

        // act
        var found = await repo.TryGetByIdAsync(Guid.NewGuid());

        // assert
        found.IsNull();
    }

    #endregion

    #region TryGetAccessByIdAsync

    [Fact]
    public async Task TryGetAccessByIdAsync_IdAbsent_ReturnsNull()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);

        // act
        var access = await repo.TryGetAccessByIdAsync(Guid.NewGuid());

        // assert
        access.IsNull();
    }

    [Fact]
    public async Task TryGetAccessByIdAsync_IdPresent_ReflectsOwnerAndPermissionsForForUser()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var other = await CreateUserAsync();
        var permissions = new[] { new MetaPackagePermission(Guid.Empty, PermissionCategory.World, Permission.Read) };
        var package = await CreatePackageAsync(repo, owner, permissions);

        // act
        var access = await repo.TryGetAccessByIdAsync(package.Id);

        // assert
        access.IsNotNull();
        var ownerAccess = access!.ForUser(owner);
        var otherAccess = access.ForUser(other);
        ownerAccess.IsOwner.IsTrue();
        otherAccess.IsWorld.IsTrue();
        otherAccess.Has(Permission.Read).IsTrue();
    }

    #endregion

    #region TryFindByTypeNameAsync

    [Fact]
    public async Task TryFindByTypeNameAsync_NoMatch_ReturnsNull()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var type = ProjectType.Register(UniqueName("type"));

        // act
        var found = await repo.TryFindByTypeNameAsync(type, UniqueName("missing"));

        // assert
        found.IsNull();
    }

    [Fact]
    public async Task TryFindByTypeNameAsync_Match_ReturnsPackage()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: []);

        // act
        var found = await repo.TryFindByTypeNameAsync(package.Type, package.Name);

        // assert
        found.IsNotNull();
        found!.Id.Is(package.Id);
    }

    [Fact]
    public async Task TryFindByTypeNameAsync_NameDiffersInCase_DoesNotMatch()
    {
        // arrange — x.Name == name is a plain equality comparison, unlike FindAllAsync's query filter,
        // so this lookup is case-sensitive.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: []);

        // act
        var found = await repo.TryFindByTypeNameAsync(package.Type, package.Name.ToUpperInvariant());

        // assert
        found.IsNull();
    }

    #endregion

    #region UpdateInfoAsync

    [Fact]
    public async Task UpdateInfoAsync_UpdatesInfoFieldsAndLeavesDownloadsAndOwnerUntouched()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: [], downloads: 7);
        var updatedInfo = new MetaPackage(
            package.Type,
            UniqueName("updated-name"),
            "2.0.0",
            "updated description",
            Instant.FromUtc(2025, 1, 1, 0, 0),
            999, // not part of IPackageInfo — must be ignored by UpdateInfoAsync
            owner.Id,
            owner,
            []
        );

        // act
        await repo.UpdateInfoAsync(package.Id, updatedInfo);

        // assert
        var reloaded = await repo.TryGetByIdAsync(package.Id);
        reloaded.IsNotNull();
        reloaded!.Name.Is(updatedInfo.Name);
        reloaded.Version.Is(updatedInfo.Version);
        reloaded.Description.Is(updatedInfo.Description);
        reloaded.Published.Is(updatedInfo.Published);
        reloaded.Downloads.Is(7);
        reloaded.OwnerId.Is(owner.Id);
    }

    [Fact]
    public async Task UpdateInfoAsync_TargetsOnlyGivenPackage_LeavesOtherPackageUntouched()
    {
        // arrange — mirrors UserRepositoryTests.UpdateApiTokenAsync_TargetsOnlyGivenUser_LeavesOthersUntouched:
        // two packages, so an unqualified UPDATE (a broken/dropped `x.Id == id` predicate) would corrupt
        // the other package's info fields instead of only the target's.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var target = await CreatePackageAsync(repo, owner, permissions: []);
        var other = await CreatePackageAsync(repo, owner, permissions: []);
        var updatedInfo = new MetaPackage(
            target.Type,
            UniqueName("updated-name"),
            "2.0.0",
            "updated description",
            Instant.FromUtc(2025, 1, 1, 0, 0),
            999,
            owner.Id,
            owner,
            []
        );

        // act
        await repo.UpdateInfoAsync(target.Id, updatedInfo);

        // assert — the other package's own info fields are untouched
        var reloadedOther = await repo.TryGetByIdAsync(other.Id);
        reloadedOther.IsNotNull();
        reloadedOther!.Name.Is(other.Name);
        reloadedOther.Version.Is(other.Version);
        reloadedOther.Description.Is(other.Description);
        reloadedOther.Published.Is(other.Published);
    }

    #endregion

    #region SetDownloadsAsync

    [Fact]
    public async Task SetDownloadsAsync_UpdatesOnlyDownloads()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: [], downloads: 1);

        // act
        await repo.SetDownloadsAsync(package.Id, 42);

        // assert
        var reloaded = await repo.TryGetByIdAsync(package.Id);
        reloaded!.Downloads.Is(42);
        reloaded.Name.Is(package.Name);
    }

    [Fact]
    public async Task SetDownloadsAsync_TargetsOnlyGivenPackage_LeavesOtherPackageDownloadsUntouched()
    {
        // arrange — two packages, so an unqualified UPDATE would bump the other package's downloads too.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var target = await CreatePackageAsync(repo, owner, permissions: [], downloads: 1);
        var other = await CreatePackageAsync(repo, owner, permissions: [], downloads: 2);

        // act
        await repo.SetDownloadsAsync(target.Id, 42);

        // assert
        var reloadedTarget = await repo.TryGetByIdAsync(target.Id);
        var reloadedOther = await repo.TryGetByIdAsync(other.Id);
        reloadedTarget!.Downloads.Is(42);
        reloadedOther!.Downloads.Is(2);
    }

    #endregion

    #region UpdatePermissionsAsync

    [Fact]
    public async Task UpdatePermissionsAsync_MultipleCategoriesProvided_UpdatesEachRowIndependently()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var permissions = new[]
        {
            new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Read),
            new MetaPackagePermission(Guid.Empty, PermissionCategory.World, Permission.Read),
        };
        var package = await CreatePackageAsync(repo, owner, permissions);

        // act
        await repo.UpdatePermissionsAsync(
            package.Id,
            [
                new MetaPackagePermission(package.Id, PermissionCategory.Owner, Permission.Read | Permission.Publish),
                new MetaPackagePermission(package.Id, PermissionCategory.World, Permission.None),
            ]
        );

        // assert
        var reloaded = await repo.TryGetByIdAsync(package.Id);
        reloaded!.Permissions.Has(2);
        reloaded
            .Permissions.Any(p =>
                p.Category == PermissionCategory.Owner && p.Permission == (Permission.Read | Permission.Publish)
            )
            .IsTrue();
        reloaded
            .Permissions.Any(p => p.Category == PermissionCategory.World && p.Permission == Permission.None)
            .IsTrue();
    }

    [Fact]
    public async Task UpdatePermissionsAsync_CategoryWithNoExistingRow_IsNoOpAndDoesNotInsert()
    {
        // arrange — UpdatePermissionsAsync is a per-category UPDATE (Db.MetaPackagePermissions.Where(...).Set(...).UpdateAsync()),
        // never an upsert, so a category with no matching row is silently skipped rather than created.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var permissions = new[] { new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Read) };
        var package = await CreatePackageAsync(repo, owner, permissions);

        // act
        await repo.UpdatePermissionsAsync(
            package.Id,
            [new MetaPackagePermission(package.Id, PermissionCategory.World, Permission.Read)]
        );

        // assert — still just the one Owner row; no World row was created
        var reloaded = await repo.TryGetByIdAsync(package.Id);
        reloaded!.Permissions.Has(1);
        reloaded.Permissions.Single().Category.Is(PermissionCategory.Owner);
    }

    [Fact]
    public async Task UpdatePermissionsAsync_TargetsOnlyGivenPackage_LeavesOtherPackagePermissionsUntouched()
    {
        // arrange — two packages sharing the same permission categories. UpdatePermissionsAsync.cs:107
        // filters each per-category UPDATE by `p.MetaPackageId == id`; dropping or flipping that predicate
        // would make the update hit BOTH packages' rows for a shared category instead of only the
        // target's, since `p.Category == permission.Category` alone can't tell the packages apart.
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var sharedPermissions = new[]
        {
            new MetaPackagePermission(Guid.Empty, PermissionCategory.Owner, Permission.Read),
            new MetaPackagePermission(Guid.Empty, PermissionCategory.World, Permission.Read),
        };
        var target = await CreatePackageAsync(repo, owner, sharedPermissions);
        var other = await CreatePackageAsync(repo, owner, sharedPermissions);

        // act
        await repo.UpdatePermissionsAsync(
            target.Id,
            [
                new MetaPackagePermission(target.Id, PermissionCategory.Owner, Permission.Read | Permission.Publish),
                new MetaPackagePermission(target.Id, PermissionCategory.World, Permission.None),
            ]
        );

        // assert — the other package's rows for the very same categories are untouched
        var reloadedOther = await repo.TryGetByIdAsync(other.Id);
        reloadedOther!.Permissions.Has(2);
        reloadedOther
            .Permissions.Any(p => p.Category == PermissionCategory.Owner && p.Permission == Permission.Read)
            .IsTrue();
        reloadedOther
            .Permissions.Any(p => p.Category == PermissionCategory.World && p.Permission == Permission.Read)
            .IsTrue();
    }

    #endregion

    #region DeleteByIdAsync

    [Fact]
    public async Task DeleteByIdAsync_ExistingPackage_RemovesIt()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: []);

        // act
        await repo.DeleteByIdAsync(package.Id);

        // assert
        var reloaded = await repo.TryGetByIdAsync(package.Id);
        reloaded.IsNull();
    }

    [Fact]
    public async Task DeleteByIdAsync_NonExistentId_IsNoOp()
    {
        // arrange
        await using var repo = _fixture.CreateMetaPackageRepository(Logger);
        var owner = await CreateUserAsync();
        var package = await CreatePackageAsync(repo, owner, permissions: []);

        // act
        await repo.DeleteByIdAsync(Guid.NewGuid());

        // assert — the real package is untouched
        var reloaded = await repo.TryGetByIdAsync(package.Id);
        reloaded.IsNotNull();
    }

    #endregion

    private static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private async Task<User> CreateUserAsync()
    {
        await using var userRepo = _fixture.CreateUserRepository(Logger);
        var user = new User(UniqueName("user"), "hash", Guid.NewGuid());
        await userRepo.CreateAsync(user);

        return user;
    }
}
