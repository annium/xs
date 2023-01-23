using System;
using Annium.Data.Models;

namespace Server.Domain.Models;

public sealed record User : IIdEntity<Guid>
{
    public Guid Id { get; private init; }
    public string Login { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Guid ApiToken { get; private set; }

    public User(
        string login,
        string passwordHash,
        Guid apiToken
    )
    {
        Id = Guid.NewGuid();
        Login = login;
        PasswordHash = passwordHash;
        ApiToken = apiToken;
    }

    internal User()
    {
    }

    public void Update(
        string login,
        string passwordHash,
        Guid apiToken
    )
    {
        Login = login;
        PasswordHash = passwordHash;
        ApiToken = apiToken;
    }
}