using System;
using System.Collections.Generic;

namespace Xs.Registry.Core.Models
{
    public class User
    {
        public string Id { get; }

        public string Name { get; }

        public string PasswordHash { get; }

        public Guid ApiToken { get; }

        public List<UserSession> Sessions { get; } = new List<UserSession>();

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
            string id,
            string name,
            string passwordHash,
            Guid apiToken,
            List<UserSession> sessions
        ) : this(name, passwordHash, apiToken)
        {
            Id = id;
            Sessions = sessions;
        }
    }

    public class UserSession
    {
        public Guid Token { get; }

        public DateTime Expires { get; }

        public UserSession(Guid token, DateTime expires)
        {
            this.Token = token;
            this.Expires = expires;
        }
    }
}