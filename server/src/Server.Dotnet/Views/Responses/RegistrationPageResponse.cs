using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server.Dotnet.Views.Responses;

internal sealed record RegistrationPageResponse(
    [property: JsonPropertyName("@id")] Uri Id,
    IReadOnlyCollection<RegistrationLeafResponse> Items,
    string Lower,
    string Upper
)
{
    public int Count => Items.Count;
}