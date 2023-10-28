using Annium.linq2db.Extensions;
using LinqToDB.Mapping;
using Server.Node.Domain;

namespace Server.Node.Internal.Db.Configurations;

internal class PackageDependencyConfiguration : IEntityConfiguration<PackageDependency>
{
    public void Configure(EntityMappingBuilder<PackageDependency> builder)
    {
        builder.HasSchemaName(Constants.Project).HasTableName("package_dependencies");
        builder.Property(x => x.PackageId).IsColumn();
        builder.Property(x => x.Type).IsColumn();
        builder.Property(x => x.Name).IsColumn();
        builder.Property(x => x.Version).IsColumn();
    }
}
