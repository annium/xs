using System;
using System.Text.Json.Serialization;

namespace Server.Dotnet.Views.Responses;

internal sealed record CatalogEntryResponse(
    [property: JsonPropertyName("@id")] Uri Id,
    [property: JsonPropertyName("id")] string Name,
    string Version
);
