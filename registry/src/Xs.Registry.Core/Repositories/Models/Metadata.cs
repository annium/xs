using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Xs.Core.Models;

namespace Xs.Registry.Core.Repositories.Models
{
    internal class Metadata
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("owner")]
        public string OwnerId { get; set; }

        [BsonElement("type")]
        public string ProjectType { get; set; }

        [BsonElement("name")]
        public string PackageName { get; set; }

        [BsonElement("permissions")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<PermissionCategory, Permission> Permissions { get; set; }

        public static explicit operator Metadata(Core.Models.Metadata src)
        {
            if (src == null)
                return null;

            var model = new Metadata();

            model.OwnerId = src.OwnerId;
            model.ProjectType = src.ProjectType.ToString();
            model.PackageName = src.PackageName;
            model.Permissions = new Dictionary<PermissionCategory, Permission>(src.Permissions);

            return model;
        }

        public static explicit operator Core.Models.Metadata(Metadata src)
        {
            if (src == null)
                return null;

            return new Core.Models.Metadata(
                src.OwnerId,
                Xs.Core.Models.ProjectType.Get(src.ProjectType),
                src.PackageName,
                src.Permissions
            );
        }
    }
}