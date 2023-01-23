using Server.Shared.Domain.Models;

namespace Server.Node.Internal;

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