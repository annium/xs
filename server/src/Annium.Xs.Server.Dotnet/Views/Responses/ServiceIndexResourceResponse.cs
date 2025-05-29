using System;
using System.Text.Json.Serialization;

namespace Annium.Xs.Server.Dotnet.Views.Responses;

internal sealed record ServiceIndexResourceResponse(
    [property: JsonPropertyName("@id")] Uri Uri,
    [property: JsonPropertyName("@type")] string Type
)
{
    public string Comment => string.Empty;
}
