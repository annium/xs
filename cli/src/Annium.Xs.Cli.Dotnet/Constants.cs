using Annium.Xs.Cli.Core.Models;

namespace Annium.Xs.Cli.Dotnet;

internal static class Constants
{
    public const string Type = "dotnet";
    public const string DefaultServer = "https://api.nuget.org";
    public const string ServerPathSuffix = "/v3/index.json";
    public static readonly ProjectType ProjectType = ProjectType.Dotnet;
}
