using Annium.linq2db.Extensions.Configuration;
using Annium.Xs.Server.Dotnet.Domain;
using LinqToDB.Mapping;

namespace Annium.Xs.Server.Dotnet.Internal.Db.Configurations;

internal class PackageDependencyConfiguration : IEntityConfiguration<PackageDependency>
{
    public void Configure(EntityMappingBuilder<PackageDependency> builder)
    {
        builder.HasSchemaName(Constants.Project).HasTableName("package_dependencies");
        builder.Property(x => x.PackageId).IsColumn();
        builder.Property(x => x.Framework).IsColumn();
        builder.Property(x => x.Name).IsColumn();
        builder.Property(x => x.Version).IsColumn();
    }
}
