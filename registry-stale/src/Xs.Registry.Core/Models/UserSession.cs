using System;
using NodaTime;

namespace Xs.Registry.Core.Models
{
    public class UserSession
    {
        public Guid Token { get; }

        public Guid UserId { get; }

        public Instant Expires { get; }

        public UserSession(
            Guid token,
            Guid userId,
            Instant expires
        )
        {
            Token = token;
            UserId = userId;
            Expires = expires;
        }
    }
}