using System;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Internal.Repositories;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Postgres-backed integration tests for the internal <see cref="UserRepository"/>, pinning
/// <see cref="UserRepository.TryFindByApiTokenAsync"/> — the exact query
/// <c>AuthorizationFilter</c> depends on for identity resolution — plus its remaining CRUD behaviour
/// against the real <c>main.users</c> schema.
/// </summary>
[Collection(PostgresCollection.Name)]
public class UserRepositoryTests : TestBase
{
    private readonly PostgresFixture _fixture;

    public UserRepositoryTests(PostgresFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _fixture = fixture;
    }

    #region TryFindByApiTokenAsync

    [Fact]
    public async Task TryFindByApiTokenAsync_TokenMatches_ReturnsUser()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);
        var user = new User(UniqueName("user"), "hash", Guid.NewGuid());
        await repo.CreateAsync(user);

        // act
        var found = await repo.TryFindByApiTokenAsync(user.ApiToken);

        // assert
        found.IsNotNull();
        found!.Id.Is(user.Id);
    }

    [Fact]
    public async Task TryFindByApiTokenAsync_TokenDoesNotMatch_ReturnsNull()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);
        var user = new User(UniqueName("user"), "hash", Guid.NewGuid());
        await repo.CreateAsync(user);

        // act
        var found = await repo.TryFindByApiTokenAsync(Guid.NewGuid());

        // assert
        found.IsNull();
    }

    [Fact]
    public async Task TryFindByApiTokenAsync_GuidEmpty_ReturnsNullAgainstNormallySeededDb()
    {
        // arrange — AuthorizationFilter can reach this repository with an all-zero token (e.g. when no
        // ITokenAccessor is registered — see AuthorizationFilterTests). Pinning the contract explicitly:
        // against a normally-seeded DB (migration 0004_seed.sql seeds one 'alex' user with a real,
        // non-zero token), Guid.Empty simply misses, like any other non-matching token.
        await using var repo = _fixture.CreateUserRepository(Logger);

        // act
        var found = await repo.TryFindByApiTokenAsync(Guid.Empty);

        // assert
        found.IsNull();
    }

    #endregion

    #region TryFindByLoginAsync

    [Fact]
    public async Task TryFindByLoginAsync_LoginMatches_ReturnsUser()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);
        var login = UniqueName("user");
        var user = new User(login, "hash", Guid.NewGuid());
        await repo.CreateAsync(user);

        // act
        var found = await repo.TryFindByLoginAsync(login);

        // assert
        found.IsNotNull();
        found!.Id.Is(user.Id);
    }

    [Fact]
    public async Task TryFindByLoginAsync_LoginDoesNotMatch_ReturnsNull()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);

        // act
        var found = await repo.TryFindByLoginAsync(UniqueName("missing"));

        // assert
        found.IsNull();
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_PersistsAllFields()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);
        var login = UniqueName("user");
        var token = Guid.NewGuid();
        var user = new User(login, "a-hash", token);

        // act
        await repo.CreateAsync(user);

        // assert
        var found = await repo.TryFindByLoginAsync(login);
        found.IsNotNull();
        found!.Login.Is(login);
        found.PasswordHash.Is("a-hash");
        found.ApiToken.Is(token);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_UpdatesLoginPasswordHashAndApiToken()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);
        var user = new User(UniqueName("user"), "old-hash", Guid.NewGuid());
        await repo.CreateAsync(user);
        var newLogin = UniqueName("user-renamed");
        var newToken = Guid.NewGuid();
        user.Update(newLogin, "new-hash", newToken);

        // act
        await repo.UpdateAsync(user);

        // assert
        var found = await repo.TryFindByApiTokenAsync(newToken);
        found.IsNotNull();
        found!.Id.Is(user.Id);
        found.Login.Is(newLogin);
        found.PasswordHash.Is("new-hash");
    }

    #endregion

    #region UpdateApiTokenAsync

    [Fact]
    public async Task UpdateApiTokenAsync_TargetsOnlyGivenUser_LeavesOthersUntouched()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);
        var target = new User(UniqueName("target"), "hash", Guid.NewGuid());
        var other = new User(UniqueName("other"), "hash", Guid.NewGuid());
        await repo.CreateAsync(target);
        await repo.CreateAsync(other);
        var newToken = Guid.NewGuid();

        // act
        await repo.UpdateApiTokenAsync(target.Id, newToken);

        // assert
        var reloadedTarget = await repo.TryFindByApiTokenAsync(newToken);
        var reloadedOther = await repo.TryFindByApiTokenAsync(other.ApiToken);
        reloadedTarget.IsNotNull();
        reloadedTarget!.Id.Is(target.Id);
        reloadedOther.IsNotNull();
        reloadedOther!.Id.Is(other.Id);
    }

    #endregion

    #region DeleteByIdAsync

    [Fact]
    public async Task DeleteByIdAsync_ExistingUser_RemovesTheRow()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);
        var user = new User(UniqueName("user"), "hash", Guid.NewGuid());
        await repo.CreateAsync(user);

        // act
        await repo.DeleteByIdAsync(user.Id);

        // assert
        var found = await repo.TryFindByApiTokenAsync(user.ApiToken);
        found.IsNull();
    }

    [Fact]
    public async Task DeleteByIdAsync_NonExistentId_IsNoOp()
    {
        // arrange
        await using var repo = _fixture.CreateUserRepository(Logger);
        var user = new User(UniqueName("user"), "hash", Guid.NewGuid());
        await repo.CreateAsync(user);

        // act — deleting an id that doesn't exist does not throw
        await repo.DeleteByIdAsync(Guid.NewGuid());

        // assert — the real user is untouched
        var found = await repo.TryFindByApiTokenAsync(user.ApiToken);
        found.IsNotNull();
    }

    #endregion

    private static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}";
}
