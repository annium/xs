using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Xs.Registry.Dotnet.Repositories.Models
{
    internal class Package
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("version")]
        public string Version { get; set; }

        [BsonElement("desc")]
        public string Description { get; set; }

        [BsonElement("deps")]
        public Dictionary<string, List<ValueTuple<string, string>>> Dependencies { get; set; }

        public static implicit operator Dotnet.Models.Package(Package src)
        {
            if (src == null)
                return null;

            return new Dotnet.Models.Package(
                src.Name,
                NuGet.Versioning.NuGetVersion.Parse(src.Version),
                src.Description,
                src.Dependencies.ToDictionary(
                    e => NuGet.Frameworks.NuGetFramework.Parse(e.Key),
                    e => e.Value.Select(d => (d.Item1, NuGet.Versioning.VersionRange.Parse(d.Item2))).ToArray().AsEnumerable()
                )
            );
        }

        public static implicit operator Package(Dotnet.Models.Package src)
        {
            if (src == null)
                return null;

            var model = new Package();

            model.Name = src.Name;
            model.Version = src.Version.ToString();
            model.Description = src.Description;
            model.Dependencies = src.Dependencies.ToDictionary(
                e => e.Key.GetShortFolderName(),
                e => e.Value.Select(d => (d.Item1, d.Item2.ToString())).ToList()
            );

            return model;
        }
    }
}