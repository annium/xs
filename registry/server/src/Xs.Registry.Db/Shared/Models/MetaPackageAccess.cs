using System;
using System.Collections.Generic;

namespace Xs.Registry.Db.Shared;

public class MetaPackageAccess
{
    private Guid OwnerId { get; }

    private IEnumerable<MetaPackagePermission> Permissions { get; }

    internal MetaPackageAccess(
        Guid ownerId,
        IEnumerable<MetaPackagePermission> permissions
    )
    {
        OwnerId = ownerId;
        Permissions = permissions;
    }

    public UserMetaPackageAccess ForUser(User user) =>
        // for empty user - assume world access
        new(user == null ? Guid.Empty : user.Id, OwnerId, Permissions);
}