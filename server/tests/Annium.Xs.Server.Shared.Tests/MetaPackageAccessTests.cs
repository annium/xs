using System;
using System.Collections.Generic;
using Annium.Testing;
using Annium.Xs.Server.Shared.Domain.Enums;
using Annium.Xs.Server.Shared.Domain.Models;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for <see cref="MetaPackageAccess"/> and <see cref="UserMetaPackageAccess"/>, pinning the
/// owner/world category resolution done by <see cref="MetaPackageAccess.ForUser"/> and the resulting
/// per-flag <see cref="UserMetaPackageAccess.Has"/> checks.
/// </summary>
public class MetaPackageAccessTests
{
    [Fact]
    public void ForUser_OwnerUser_ResolvesOwnerCategoryAndPermissions()
    {
        // arrange
        var owner = new User("owner", "hash", Guid.NewGuid());
        var permissions = new List<MetaPackagePermission>
        {
            new(Guid.Empty, PermissionCategory.Owner, Permission.Read | Permission.Publish),
            new(Guid.Empty, PermissionCategory.World, Permission.None),
        };
        var access = new MetaPackageAccess(owner.Id, permissions);

        // act
        var userAccess = access.ForUser(owner);

        // assert
        userAccess.IsOwner.IsTrue();
        userAccess.IsWorld.IsFalse();
        userAccess.Has(Permission.Read).IsTrue();
        userAccess.Has(Permission.Publish).IsTrue();
        userAccess.Has(Permission.Unpublish).IsFalse();
    }

    [Fact]
    public void ForUser_NonOwnerUser_ResolvesWorldCategoryAndPermissions()
    {
        // arrange
        var owner = new User("owner", "hash", Guid.NewGuid());
        var other = new User("other", "hash", Guid.NewGuid());
        var permissions = new List<MetaPackagePermission>
        {
            new(Guid.Empty, PermissionCategory.Owner, Permission.Read | Permission.Publish),
            new(Guid.Empty, PermissionCategory.World, Permission.Read),
        };
        var access = new MetaPackageAccess(owner.Id, permissions);

        // act
        var userAccess = access.ForUser(other);

        // assert
        userAccess.IsOwner.IsFalse();
        userAccess.IsWorld.IsTrue();
        userAccess.Has(Permission.Read).IsTrue();
        userAccess.Has(Permission.Publish).IsFalse();
    }

    [Fact]
    public void ForUser_NullUser_ResolvesWorldAccess()
    {
        // arrange — MetaPackageAccess.ForUser's own comment states "for empty user - assume world
        // access"; this pins that a null user resolves via Guid.Empty as the user id, which (as long as
        // no owner happens to have Guid.Empty as their id) lands in the World category.
        var owner = new User("owner", "hash", Guid.NewGuid());
        var permissions = new List<MetaPackagePermission>
        {
            new(Guid.Empty, PermissionCategory.Owner, Permission.Read | Permission.Publish),
            new(Guid.Empty, PermissionCategory.World, Permission.Read),
        };
        var access = new MetaPackageAccess(owner.Id, permissions);

        // act
        var userAccess = access.ForUser(null);

        // assert
        userAccess.IsOwner.IsFalse();
        userAccess.IsWorld.IsTrue();
        userAccess.Has(Permission.Read).IsTrue();
    }

    [Fact]
    public void ForUser_NoPermissionRowForResolvedCategory_DefaultsToNoneAndGrantsNothing()
    {
        // arrange — only a World row is present; the Owner category has no matching row at all.
        var owner = new User("owner", "hash", Guid.NewGuid());
        var permissions = new List<MetaPackagePermission>
        {
            new(Guid.Empty, PermissionCategory.World, Permission.Read),
        };
        var access = new MetaPackageAccess(owner.Id, permissions);

        // act
        var userAccess = access.ForUser(owner);

        // assert
        userAccess.IsOwner.IsTrue();
        userAccess.Has(Permission.Read).IsFalse();
        userAccess.Has(Permission.Publish).IsFalse();
        userAccess.Has(Permission.Unpublish).IsFalse();
    }

    [Fact]
    public void Has_FlagCombinationGrantsExactlyTheGrantedFlags()
    {
        // arrange
        var owner = new User("owner", "hash", Guid.NewGuid());
        var permissions = new List<MetaPackagePermission>
        {
            new(Guid.Empty, PermissionCategory.Owner, Permission.Read | Permission.Publish),
        };
        var access = new MetaPackageAccess(owner.Id, permissions);

        // act
        var userAccess = access.ForUser(owner);

        // assert
        userAccess.Has(Permission.Read).IsTrue();
        userAccess.Has(Permission.Unpublish).IsFalse();
        userAccess.Has(Permission.Read | Permission.Publish).IsTrue();
    }
}
