using System;
using Newtonsoft.Json;

namespace Xs.Registry.Dotnet.Views;

internal class RegistrationLeafView
{
    [JsonProperty("@id")]
    public Uri Id { get; }

    public CatalogEntryView CatalogEntry { get; }

    public Uri PackageContent { get; }

    public RegistrationLeafView(
        Uri id,
        CatalogEntryView catalogEntry,
        Uri packageContent
    )
    {
        Id = id;
        CatalogEntry = catalogEntry;
        PackageContent = packageContent;
    }
}