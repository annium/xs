using System;

namespace Server.Domain.Models;

public sealed record User
{
    public Guid Id { get; }

    public string Name { get; set; }

    public string PasswordHash { get; set; }

    public Guid ApiToken { get; set; }

    public User(
        string name,
        string passwordHash,
        Guid apiToken
    )
    {
        Name = name;
        PasswordHash = passwordHash;
        ApiToken = apiToken;
    }

    internal User(
        Guid id,
        string name,
        string passwordHash,
        Guid apiToken
    ) : this(name, passwordHash, apiToken)
    {
        Id = id;
    }
}