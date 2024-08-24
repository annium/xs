using Xx.Cli.Core.Models;

namespace Xx.Cli.Dotnet;

internal static class Constants
{
    public const string DefaultServer = "https://api.nuget.org";
    public const string ServerPathSuffix = "/v3/index.json";
    public static readonly ProjectType ProjectType = ProjectType.Dotnet;
}
