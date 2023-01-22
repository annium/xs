using System;
using Annium.Data.Models;

namespace Server.Domain.Models;

public sealed record User : IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public string Name { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Guid ApiToken { get; private set; }

    public User(
        string name,
        string passwordHash,
        Guid apiToken
    )
    {
        Id = Guid.NewGuid();
        Name = name;
        PasswordHash = passwordHash;
        ApiToken = apiToken;
    }

    internal User()
    {
    }

    public void Update(
        string name,
        string passwordHash,
        Guid apiToken
    )
    {
        Name = name;
        PasswordHash = passwordHash;
        ApiToken = apiToken;
    }
}