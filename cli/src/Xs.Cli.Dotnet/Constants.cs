using Xs.Cli.Core.Models;

namespace Xs.Cli.Dotnet;

internal static class Constants
{
    public const string DefaultServer = "https://api.nuget.org";

    public const string ServerPathSuffix = "/v3/index.json";

    public static readonly ProjectType ProjectType;

    internal const string ProjectTypeString = "dotnet";

    static Constants()
    {
        ProjectType.Register(ProjectTypeString);
        ProjectType = ProjectType.Get(ProjectTypeString);
    }
}