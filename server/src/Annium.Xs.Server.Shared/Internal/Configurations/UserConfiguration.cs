using System;
using Annium.linq2db.Extensions.Configuration;
using Annium.Xs.Server.Shared.Domain.Models;
using LinqToDB.Mapping;

namespace Annium.Xs.Server.Shared.Internal.Configurations;

internal class UserConfiguration : IIdEntityConfiguration<User, Guid>
{
    public void Configure(EntityMappingBuilder<User> builder)
    {
        this.ConfigureId(builder);
        builder.HasSchemaName(Constants.Schema).HasTableName("users");
        builder.Property(x => x.Login).IsColumn();
        builder.Property(x => x.PasswordHash).IsColumn();
        builder.Property(x => x.ApiToken).IsColumn();
    }
}
