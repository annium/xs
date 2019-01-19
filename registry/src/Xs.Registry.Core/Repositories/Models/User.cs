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
        public string Token { get; set; }

        public static implicit operator User(Xs.Core.Models.User src)
        {
            if (src == null)
                return null;

            var model = new User();

            if (src.Id != null)
                model.Id = src.Id;
            model.Name = src.Name;
            model.PasswordHash = src.PasswordHash;
            model.Token = src.Token;

            return model;
        }

        public static implicit operator Xs.Core.Models.User(User src)
        {
            if (src == null)
                return null;

            return new Xs.Core.Models.User(
                src.Id,
                src.Name,
                src.PasswordHash,
                src.Token
            );
        }
    }
}