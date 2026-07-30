using System;
using System.Linq;
using Annium.Testing;
using Annium.Xs.Server.Shared.Domain.Enums;
using Annium.Xs.Server.Shared.Domain.Interfaces;
using Annium.Xs.Server.Shared.Domain.Models;
using Annium.Xs.Server.Shared.Internal.Tools;
using NodaTime;
using Xunit;

namespace Annium.Xs.Server.Shared.Tests;

/// <summary>
/// Tests for the internal <see cref="MetaPackageTool"/>, pinning the exact owner/world permission
/// grants <see cref="MetaPackageTool.Generate"/> assigns to a freshly-created meta-package. This is the
/// source of truth that <c>Annium.Xs.Server.Abstractions.Tests</c>' hand-mirrored
/// <c>FakeMetaPackageTool</c> is checked against — if this test ever fails after a change to the real
/// tool, the fake has silently drifted from it and needs updating too.
/// </summary>
public class MetaPackageToolTests
{
    private static readonly ProjectType _projectType = ProjectType.Register("xs-shared-tests-meta-package-tool");

    private readonly MetaPackageTool _tool = new();

    [Fact]
    public void Generate_AssignsOwnerReadPublishAndWorldNone()
    {
        // arrange
        var user = new User("owner", "hash", Guid.NewGuid());
        var packageInfo = new FakePackageInfo("pkg-a", "1.0.0", "description", Instant.FromUtc(2024, 1, 1, 0, 0));

        // act
        var metaPackage = _tool.Generate(user, _projectType, packageInfo);

        // assert
        var ownerPermission = metaPackage.Permissions.Single(p => p.Category == PermissionCategory.Owner);
        var worldPermission = metaPackage.Permissions.Single(p => p.Category == PermissionCategory.World);
        ownerPermission.Permission.Is(Permission.Read | Permission.Publish);
        worldPermission.Permission.Is(Permission.None);
    }

    [Fact]
    public void Generate_BothPermissionsReferenceTheGeneratedMetaPackageId()
    {
        // arrange
        var user = new User("owner", "hash", Guid.NewGuid());
        var packageInfo = new FakePackageInfo("pkg-a", "1.0.0", "description", Instant.FromUtc(2024, 1, 1, 0, 0));

        // act
        var metaPackage = _tool.Generate(user, _projectType, packageInfo);

        // assert
        metaPackage.Permissions.Has(2);
        metaPackage.Permissions.All(p => p.MetaPackageId == metaPackage.Id).IsTrue();
    }

    [Fact]
    public void GetAccess_ResolvesGeneratingUserAsOwnerAndEveryoneElseAsWorld()
    {
        // arrange
        var user = new User("owner", "hash", Guid.NewGuid());
        var other = new User("other", "hash", Guid.NewGuid());
        var packageInfo = new FakePackageInfo("pkg-a", "1.0.0", "description", Instant.FromUtc(2024, 1, 1, 0, 0));
        var metaPackage = _tool.Generate(user, _projectType, packageInfo);

        // act
        var access = _tool.GetAccess(metaPackage);

        // assert — the generating user gets the owner grant (Read|Publish)...
        access.ForUser(user).Has(Permission.Read | Permission.Publish).IsTrue();
        // ...while anyone else gets the world grant, which Generate sets to None
        access.ForUser(other).Has(Permission.Read).IsFalse();
    }

    private sealed class FakePackageInfo : IPackageInfo
    {
        public FakePackageInfo(string name, string version, string description, Instant published)
        {
            Name = name;
            Version = version;
            Description = description;
            Published = published;
        }

        public string Name { get; }
        public string Version { get; }
        public string Description { get; }
        public Instant Published { get; }
    }
}
