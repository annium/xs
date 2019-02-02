using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Xs.Core.Helpers;
using Xs.Registry.Core.Models;

namespace Xs.Registry.Dotnet.Db.Models
{
    internal class Package : IPackage
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        public string MetaPackageId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("version")]
        public string Version { get; set; }

        [BsonElement("desc")]
        public string Description { get; set; }

        [BsonElement("deps")]
        public List<PackageDependencies> Dependencies { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("published")]
        public DateTime Published { get; set; }

        [BsonElement("downloads")]
        public uint Downloads { get; set; }

        public static explicit operator Package(Dotnet.Models.Package src)
        {
            if (src == null)
                return null;

            var model = new Package();

            if (src.Id != null)
                model.Id = src.Id;
            model.MetaPackageId = src.MetaPackageId;
            model.Name = src.Name;
            model.Version = src.Version;
            model.Description = src.Description;
            model.Dependencies = src.Dependencies
                .Select(e => new PackageDependencies()
                {
                    Framework = e.Key.GetShortFolderName(),
                        Dependencies = e.Value.ToDictionary(d => d.Key, d => d.Value.ToString())
                })
                .ToList();
            model.Published = src.Published;
            model.Downloads = src.Downloads;

            return model;
        }

        public static explicit operator Dotnet.Models.Package(Package src)
        {
            if (src == null)
                return null;

            return new Dotnet.Models.Package(
                src.Id,
                src.MetaPackageId,
                src.Name,
                NuGet.Versioning.NuGetVersion.Parse(src.Version),
                src.Description,
                src.Dependencies.ToDictionary(
                    e => NuGet.Frameworks.NuGetFramework.Parse(e.Framework),
                    e => e.Dependencies.ToDictionary(d => d.Key, d => NuGet.Versioning.VersionRange.Parse(d.Value)).ToReadOnly()
                ),
                src.Published,
                src.Downloads
            );
        }
    }
}