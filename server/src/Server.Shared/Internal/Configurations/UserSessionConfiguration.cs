using System;
using Annium.linq2db.Extensions.Configuration;
using LinqToDB.Mapping;
using Server.Domain.Models;

namespace Server.Shared.Internal.Configurations;

internal class UserSessionConfiguration : IdEntityConfiguration<UserSession, Guid>
{
    public override void Configure(EntityMappingBuilder<UserSession> builder)
    {
        builder.HasSchemaName(Constants.Schema).HasTableName("user_sessions");
        base.Configure(builder);
        builder.Association(x => x.User, x => x.UserId, x => x.Id, canBeNull: false);
        builder.Property(x => x.Token).IsColumn();
        builder.Property(x => x.Expires).IsColumn();
    }
}