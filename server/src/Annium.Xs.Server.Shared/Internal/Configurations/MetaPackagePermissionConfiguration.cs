using Annium.linq2db.Extensions;
using Annium.Xs.Server.Shared.Domain.Models;
using LinqToDB.Mapping;

namespace Annium.Xs.Server.Shared.Internal.Configurations;

internal class MetaPackagePermissionConfiguration : IEntityConfiguration<MetaPackagePermission>
{
    public void Configure(EntityMappingBuilder<MetaPackagePermission> builder)
    {
        builder.HasSchemaName(Constants.Schema).HasTableName("meta_package_permissions");
        builder.Property(x => x.MetaPackageId).IsColumn();
        builder.Property(x => x.Category).IsColumn();
        builder.Property(x => x.Permission).IsColumn();
    }
}
