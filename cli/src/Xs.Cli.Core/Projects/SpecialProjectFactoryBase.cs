using System.IO;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects;

public class SpecialProjectFactoryBase<TProject> where TProject : class, IProject
{
    protected Dependency<IProject> GetProjectDependencyMock(
        FileInfo location,
        Dependency<string> reference
    )
    {
        var locationParent = location.DirectoryName ?? throw new DirectoryNotFoundException($"File {location} has no parent directory");
        var file = Path.GetFullPath(Path.Combine(locationParent, reference.Value));
        var fileParent = Directory.GetParent(file) ?? throw new DirectoryNotFoundException($"File {location} has no parent directory");
        var directory = fileParent.FullName;

        var dependency = new ProjectMock<TProject>(
            location.Name,
            new Version(0, 0, 0, string.Empty),
            string.Empty,
            directory,
            file
        );

        return new Dependency<IProject>(reference.Type, dependency);
    }
}