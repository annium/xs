using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Xs.Registry.Core.Repositories.Models
{
    internal class User
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("pass")]
        public string PasswordHash { get; set; }

        [BsonElement("token")]
        public Guid ApiToken { get; set; }

        [BsonElement("sessions")]
        public List<UserSession> Sessions { get; set; }

        public static explicit operator User(Core.Models.User src)
        {
            if (src == null)
                return null;

            var model = new User();

            if (src.Id != null)
                model.Id = src.Id;
            model.Name = src.Name;
            model.PasswordHash = src.PasswordHash;
            model.ApiToken = src.ApiToken;

            return model;
        }

        public static explicit operator Core.Models.User(User src)
        {
            if (src == null)
                return null;

            return new Core.Models.User(
                src.Id,
                src.Name,
                src.PasswordHash,
                src.ApiToken,
                src.Sessions.Select(e => (Core.Models.UserSession) e).ToList()
            );
        }
    }

    internal class UserSession
    {
        public static explicit operator UserSession(Core.Models.UserSession src)
        {
            var model = new UserSession();

            model.Token = src.Token;
            model.Expires = src.Expires;

            return model;
        }

        [BsonElement("token")]
        public Guid Token { get; set; }

        [BsonElement("expires")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Expires { get; set; }

        public static explicit operator Core.Models.UserSession(UserSession src)
        {
            if (src == null)
                return null;

            return new Core.Models.UserSession(
                src.Token,
                src.Expires
            );
        }
    }
}