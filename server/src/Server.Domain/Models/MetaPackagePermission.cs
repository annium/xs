using System;
using Annium.Data.Models;
using Server.Domain.Enums;

namespace Server.Domain.Models;

public sealed record MetaPackagePermission : IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public PermissionCategory Category { get; private init; }
    public Permission Permission { get; private init; }

    public MetaPackagePermission(
        PermissionCategory category,
        Permission permission
    )
    {
        Id = Guid.NewGuid();
        Category = category;
        Permission = permission;
    }

    internal MetaPackagePermission()
    {
    }
}