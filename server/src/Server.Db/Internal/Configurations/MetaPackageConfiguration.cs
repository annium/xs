using System;
using Annium.linq2db.Extensions.Configuration;
using LinqToDB.Mapping;
using Server.Domain.Models;

namespace Server.Db.Internal.Configurations;

internal class MetaPackageConfiguration : IdEntityConfiguration<MetaPackage, Guid>
{
    public override void Configure(EntityMappingBuilder<MetaPackage> builder)
    {
        builder.HasSchemaName(Constants.Schema).HasTableName("metapackages");
        base.Configure(builder);
        builder.Property(x => x.Type).IsColumn().HasConversion(x => x.ToString(), x => ProjectType.Get(x), handlesNulls: false);
        builder.Property(x => x.Name).IsColumn();
        builder.Property(x => x.Version).IsColumn();
        builder.Property(x => x.Description).IsColumn();
        builder.Property(x => x.Published).IsColumn();
        builder.Property(x => x.Downloads).IsColumn();
        builder.Association(x => x.Owner, x => x.OwnerId, x => x.Id, canBeNull: false);
        builder.Association(x => x.Permissions, x => x.Id, x => x.MetaPackageId, canBeNull: false);
    }
}