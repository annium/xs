using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Xs.Core.Models;

namespace Xs.Registry.Core.Db.Models
{
    internal class MetaPackage
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("owner")]
        public string OwnerId { get; set; }

        [BsonElement("permissions")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<PermissionCategory, Permission> Permissions { get; set; }

        public static explicit operator MetaPackage(Core.Models.MetaPackage src)
        {
            if (src == null)
                return null;

            var model = new MetaPackage();

            if (src.Id != null)
                model.Id = src.Id;
            model.OwnerId = src.OwnerId;
            model.Permissions = new Dictionary<PermissionCategory, Permission>(src.Permissions);

            return model;
        }

        public static explicit operator Core.Models.MetaPackage(MetaPackage src)
        {
            if (src == null)
                return null;

            return new Core.Models.MetaPackage(
                src.Id,
                src.OwnerId,
                src.Permissions
            );
        }
    }
}