using System.Collections.Generic;

namespace Server.Dotnet.Views.Responses;

internal sealed record RegistrationIndexResponse(IReadOnlyCollection<RegistrationPageResponse> Items)
{
    public int Count => Items.Count;
}
