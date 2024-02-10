using System;
using Annium.linq2db.Extensions;
using LinqToDB.Mapping;
using Server.Node.Domain;

namespace Server.Node.Internal.Db.Configurations;

internal class PackageConfiguration : IIdEntityConfiguration<Package, Guid>
{
    public void Configure(EntityMappingBuilder<Package> builder)
    {
        this.ConfigureId(builder);
        builder.HasSchemaName(Constants.Project).HasTableName("packages");
        builder.Association(x => x.MetaPackage, x => x.MetaPackageId, x => x.Id, canBeNull: false);
        builder.Property(x => x.Name).IsColumn();
        builder.Property(x => x.Version).IsColumn();
        builder.Property(x => x.Description).IsColumn();
        builder.Property(x => x.Published).IsColumn();
        builder.Property(x => x.Downloads).IsColumn();
        builder.Property(x => x.Main).IsColumn();
        builder.Property(x => x.Shasum).IsColumn();
        builder.Property(x => x.Integrity).IsColumn();
        builder.Association(x => x.Dependencies, x => x.Id, x => x.PackageId, canBeNull: false);
    }
}
