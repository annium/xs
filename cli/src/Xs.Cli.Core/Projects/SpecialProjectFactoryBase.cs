using System.IO;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    public class SpecialProjectFactoryBase<TProject> where TProject : class, IProject
    {
        protected Dependency<IProject> GetProjectDependencyMock(
            string project,
            FileInfo location,
            Dependency<string> reference
        )
        {
            var file = Path.GetFullPath(Path.Combine(location.DirectoryName, reference.Value));
            var directory = Directory.GetParent(file).FullName;

            var dependency = new ProjectMock<TProject>(
                location.Name,
                new Models.Version(0, 0, 0, string.Empty),
                string.Empty,
                directory,
                file
            );

            return new Dependency<IProject>(reference.Type, dependency);
        }
    }
}