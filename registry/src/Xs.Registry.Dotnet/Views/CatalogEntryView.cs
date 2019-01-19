using System;
using Newtonsoft.Json;

namespace Xs.Registry.Dotnet.Views
{
    internal class CatalogEntryView
    {
        [JsonProperty("@id")]
        public Uri Id { get; }

        [JsonProperty("id")]
        public string Name { get; }

        public string Version { get; }

        public CatalogEntryView(
            Uri id,
            string name,
            string version
        )
        {
            Id = id;
            Name = name;
            Version = version;
        }
    }
}