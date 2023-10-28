using System;
using Annium.linq2db.Extensions;
using LinqToDB.Mapping;
using Server.Shared.Domain.Models;

namespace Server.Shared.Internal.Configurations;

internal class UserConfiguration : IdEntityConfiguration<User, Guid>
{
    public override void Configure(EntityMappingBuilder<User> builder)
    {
        builder.HasSchemaName(Constants.Schema).HasTableName("users");
        base.Configure(builder);
        builder.Property(x => x.Login).IsColumn();
        builder.Property(x => x.PasswordHash).IsColumn();
        builder.Property(x => x.ApiToken).IsColumn();
    }
}
