using Annium.linq2db.Extensions.Configuration;
using LinqToDB.Mapping;
using Server.Domain.Models;

namespace Server.Db.Internal.Configurations;

internal class MetaPackagePermissionConfiguration : IEntityConfiguration<MetaPackagePermission>
{
    public void Configure(EntityMappingBuilder<MetaPackagePermission> builder)
    {
        builder.HasSchemaName(Constants.Schema).HasTableName("metapackage_permissions");
        builder.Property(x => x.MetaPackageId).IsColumn();
        builder.Property(x => x.Category).IsColumn();
        builder.Property(x => x.Permission).IsColumn();
    }
}