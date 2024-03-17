using System.Text;
using Xs.Cli.Core.Projects;

namespace Xs.Commands.Ls;

internal static class ReferenceExtensions
{
    public static string Describe(this IProject project, bool writePath, bool writeAttributes)
    {
        if (writePath)
            return project.File;

        if (!writeAttributes)
            return project.Name;

        var sb = new StringBuilder();
        sb.Append(project.Name);
        if (project is IPublishableProject)
            sb.Append(" [Publish]");

        if (project is ITestableProject)
            sb.Append(" [Test]");

        return sb.ToString();
    }
}
