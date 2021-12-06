using Xs.Cli.Core.Models;

namespace Xs.Cli.Node;

internal static class Constants
{
    public const string DefaultServer = "https://registry.npmjs.com";
    public static readonly ProjectType ProjectType;
    internal const string ProjectTypeString = "node";

    static Constants()
    {
        ProjectType.Register(ProjectTypeString);
        ProjectType = ProjectType.Get(ProjectTypeString);
    }
}