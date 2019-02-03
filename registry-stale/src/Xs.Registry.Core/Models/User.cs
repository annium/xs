using System;

namespace Xs.Registry.Core.Models
{
    public class User
    {
        public Guid Id { get; } = Guid.NewGuid();

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
}