using System;
using System.Collections.Generic;

namespace Server.Shared.Domain.Models;

public sealed record MetaPackageAccess
{
    private Guid OwnerId { get; }
    private IReadOnlyCollection<MetaPackagePermission> Permissions { get; }

    public MetaPackageAccess(Guid ownerId, IReadOnlyCollection<MetaPackagePermission> permissions)
    {
        OwnerId = ownerId;
        Permissions = permissions;
    }

    public UserMetaPackageAccess ForUser(User? user) =>
        // for empty user - assume world access
        new(user?.Id ?? Guid.Empty, OwnerId, Permissions);
}
