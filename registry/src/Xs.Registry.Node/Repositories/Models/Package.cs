using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Node.Repositories.Models
{
    internal class Package : IPackage
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("version")]
        public string Version { get; set; }

        [BsonElement("desc")]
        public string Description { get; set; }

        [BsonElement("main")]
        public string Main { get; set; }

        [BsonElement("deps")]
        public Dictionary<string, string> Dependencies { get; set; }

        [BsonElement("devDeps")]
        public Dictionary<string, string> DevDependencies { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("published")]
        public DateTime Published { get; set; }

        [BsonElement("downloads")]
        public uint Downloads { get; set; }

        [BsonElement("shasum")]
        public string Shasum { get; set; }

        [BsonElement("intergity")]
        public string Integrity { get; set; }

        public static explicit operator Node.Models.Package(Package src)
        {
            if (src == null)
                return null;

            return new Node.Models.Package(
                Node.Models.PackageName.Parse(src.Name),
                src.Version,
                src.Description,
                src.Main,
                src.Dependencies,
                src.DevDependencies,
                src.Published,
                src.Downloads,
                src.Shasum,
                src.Integrity
            );
        }

        public static explicit operator Package(Node.Models.Package src)
        {
            if (src == null)
                return null;

            var model = new Package();

            model.Name = src.Name.ToString();
            model.Version = src.Version;
            model.Description = src.Description;
            model.Main = src.Main;
            model.Dependencies = new Dictionary<string, string>(src.Dependencies);
            model.DevDependencies = new Dictionary<string, string>(src.DevDependencies);
            model.Published = src.Published;
            model.Downloads = src.Downloads;
            model.Shasum = src.Shasum;
            model.Integrity = src.Integrity;

            return model;
        }
    }
}