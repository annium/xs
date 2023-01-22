using System;
using Annium.linq2db.Extensions.Configuration;
using LinqToDB.Mapping;
using Server.Domain.Models;

namespace Server.Db.Internal.Configurations;

internal class UserConfiguration : IdEntityConfiguration<User, Guid>
{
    public override void Configure(EntityMappingBuilder<User> builder)
    {
        builder.HasSchemaName(Constants.Schema).HasTableName("users");
        base.Configure(builder);
        builder.Property(x => x.Name).IsColumn();
        builder.Property(x => x.PasswordHash).IsColumn();
        builder.Property(x => x.ApiToken).IsColumn();
    }
}