using System.IO;
using Xx.Cli.Core.Models;

namespace Xx.Cli.Core.Projects;

public class PlatformProjectFactoryBase
{
    protected Dependency<IProject> GetProjectDependencyMock(FileInfo location, Dependency<string> reference)
    {
        var locationParent =
            location.DirectoryName ?? throw new DirectoryNotFoundException($"File {location} has no parent directory");
        var file = Path.GetFullPath(Path.Combine(locationParent, reference.Value));
        var fileParent =
            Directory.GetParent(file)
            ?? throw new DirectoryNotFoundException($"File {location} has no parent directory");
        var directory = fileParent.FullName;

        var dependency = new ProjectMock(
            location.Name,
            new Version(0, 0, 0, string.Empty),
            string.Empty,
            directory,
            file
        );

        return new Dependency<IProject>(reference.Type, dependency);
    }
}
