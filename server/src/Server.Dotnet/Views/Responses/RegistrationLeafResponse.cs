using System;
using System.Text.Json.Serialization;

namespace Server.Dotnet.Views.Responses;

internal sealed record RegistrationLeafResponse(
    [property: JsonPropertyName("@id")] Uri Id,
    CatalogEntryResponse CatalogEntry,
    Uri PackageContent
);