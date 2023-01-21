using Server.Domain.Models;

namespace Server.Dotnet;

internal static class Constants
{
    public const string Project = "dotnet";
    public static readonly ProjectType ProjectType;

    static Constants()
    {
        ProjectType.Register(Project);
        ProjectType = ProjectType.Get(Project);
    }
}