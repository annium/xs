using System;
using Annium.Testing;
using Annium.Xs.Server.Shared.Domain.Models;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="User.Update"/>, pinning that it overwrites login/password-hash/api-token while
/// leaving the identity (<see cref="User.Id"/>) untouched.
/// </summary>
public class UserTests
{
    [Fact]
    public void Update_OverwritesLoginPasswordHashAndApiToken_ButLeavesIdUnchanged()
    {
        // arrange
        var user = new User("old-login", "old-hash", Guid.NewGuid());
        var originalId = user.Id;
        var newToken = Guid.NewGuid();

        // act
        user.Update("new-login", "new-hash", newToken);

        // assert
        user.Id.Is(originalId);
        user.Login.Is("new-login");
        user.PasswordHash.Is("new-hash");
        user.ApiToken.Is(newToken);
    }
}
