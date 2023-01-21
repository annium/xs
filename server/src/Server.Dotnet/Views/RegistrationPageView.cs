using System;
using Newtonsoft.Json;

namespace Xs.Registry.Dotnet.Views;

internal class RegistrationPageView
{
    [JsonProperty("@id")]
    public Uri Id { get; }

    public int Count => Items.Length;

    public RegistrationLeafView[] Items { get; }

    public string Lower { get; }

    public string Upper { get; }

    public RegistrationPageView(
        Uri id,
        RegistrationLeafView[] items,
        string lower,
        string upper
    )
    {
        Id = id;
        Items = items;
        Lower = lower;
        Upper = upper;
    }
}