using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects
{
    internal class ProjectFactory : IProjectFactory
    {
        private readonly IEnumerable<ISpecialProjectFactory> factories;

        public ProjectFactory(
            IEnumerable<ISpecialProjectFactory> factories
        )
        {
            this.factories = factories;
        }

        public bool IsProjectDirectory(string directory)
        {
            return factories.Any(e => e.IsProjectDirectory(directory));
        }

        public bool IsProjectFile(string file)
        {
            return factories.Any(e => e.IsProjectFile(file));
        }

        public bool IsTrackablePath(string path) => factories.Any(e => e.IsTrackablePath(path));

        public IProject CreateProject(
            string directory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies
        )
        {
            var factory = factories.FirstOrDefault(e => e.IsProjectDirectory(directory));
            if (factory == null)
                return null;

            return factory.CreateProject(
                directory,
                projects.Where(e => e.Type == factory.Type),
                dependencies.Where(e => e.Type == factory.Type)
            );
        }
    }
}