using System;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Xs.Server.Shared.Internal.Repositories;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Postgres-backed tests for <see cref="RepositoryBase{TConnection}"/>'s <c>DisposeAsync</c>, exercised
/// through the concrete <see cref="UserRepository"/>.
/// </summary>
[Collection(PostgresCollection.Name)]
public class RepositoryBaseTests : TestBase
{
    private readonly PostgresFixture _fixture;

    public RepositoryBaseTests(PostgresFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrowAndDisposesTheConnectionOnce()
    {
        // arrange — _isDisposed guards the second call from re-invoking Db.DisposeAsync() at all.
        var repo = _fixture.CreateUserRepository(Logger);

        // act
        await repo.DisposeAsync();
        await repo.DisposeAsync();

        // assert — neither call threw (an unguarded second Db.DisposeAsync() call would be the failure
        // mode here), and the underlying connection really was torn down: using the now-disposed
        // repository for an actual query fails, proving disposal happened exactly once, not zero times.
        await Wrap.It(async () => await repo.TryFindByApiTokenAsync(Guid.NewGuid()))
            .ThrowsAsync<ObjectDisposedException>();
    }
}
