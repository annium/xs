using Server.Db.Shared.Models;

namespace Server.Node;

internal static class Constants
{
    public const string Project = "node";
    public static readonly ProjectType ProjectType;

    static Constants()
    {
        ProjectType.Register(Project);
        ProjectType = ProjectType.Get(Project);
    }
}