using System;
using Annium.linq2db.Extensions.Configuration;
using LinqToDB.Mapping;
using Server.Domain.Models;

namespace Server.Db.Internal.Configurations;

internal class MetaPackagePermissionConfiguration : IdEntityConfiguration<MetaPackagePermission, Guid>
{
    public override void Configure(EntityMappingBuilder<MetaPackagePermission> builder)
    {
        builder.HasSchemaName(Constants.Schema).HasTableName("meta_package_permissions");
        base.Configure(builder);
        builder.Property(x => x.MetaPackageId).IsColumn();
        builder.Property(x => x.Category).IsColumn();
        builder.Property(x => x.Permission).IsColumn();
    }
}