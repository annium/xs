using System;
using Newtonsoft.Json;

namespace Xs.Registry.Dotnet.Views;

internal class ServiceIndexResourceView
{
    [JsonProperty("@id")]
    public Uri Uri { get; set; }

    [JsonProperty("@type")]
    public string Type { get; set; }

    public string Comment => string.Empty;
}