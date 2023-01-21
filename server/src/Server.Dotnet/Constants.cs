using Xs.Registry.Db.Shared.Models;

namespace Server.Dotnet;

internal static class Constants
{
    public static readonly ProjectType ProjectType;

    static Constants()
    {
        var type = "dotnet";
        ProjectType.Register(type);
        ProjectType = ProjectType.Get(type);
    }
}