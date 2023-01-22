using System;
using Annium.Data.Models;
using NodaTime;

namespace Server.Domain.Models;

public sealed record UserSession : IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public Guid Token { get; private init; }
    public Instant Expires { get; private init; }

    public UserSession(
        Guid userId,
        Guid token,
        Instant expires
    )
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        Expires = expires;
    }

    internal UserSession()
    {
    }
}