using System;
using System.Collections.Generic;

namespace Server.Domain.Models;

public sealed record MetaPackageAccess
{
    private Guid OwnerId { get; }

    private IEnumerable<MetaPackagePermission> Permissions { get; }

    public MetaPackageAccess(
        Guid ownerId,
        IEnumerable<MetaPackagePermission> permissions
    )
    {
        OwnerId = ownerId;
        Permissions = permissions;
    }

    public UserMetaPackageAccess ForUser(User user) =>
        // for empty user - assume world access
        new(user is null ? Guid.Empty : user.Id, OwnerId, Permissions);
}