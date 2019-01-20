using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Xs.Registry.Dotnet.Repositories.Models
{
    internal class PackageDependencies
    {
        [BsonElement("framework")]
        public string Framework { get; set; }

        [BsonElement("deps")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, string> Dependencies { get; set; }
    }
}