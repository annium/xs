using Annium.Xs.Server.Shared.Domain.Models;

namespace Annium.Xs.Server.Dotnet.Internal;

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
