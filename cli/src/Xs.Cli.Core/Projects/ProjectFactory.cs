using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Commands;
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

        public ISpecialProjectFactory FindFactory(string directory)
        {
            return factories.FirstOrDefault(e => e.IsProjectDirectory(directory));
        }

        public bool IsProjectFile(string file)
        {
            return factories.Any(e => e.IsProjectFile(file));
        }

        public IProject CreateProject(
            string directory,
            ISpecialProjectFactory factory,
            IEnumerable<IProject> projects,
            IEnumerable<Dependency> dependencies,
            DiscoverConfiguration configuration
        )
        {
            var project = factory.CreateProject(
                directory,
                projects.Where(e => e.Type == factory.Type),
                dependencies.Where(e => e.Type == factory.Type),
                configuration
            );

            if (projects.Any(p => p.Name == project.Name))
                throw new InvalidOperationException($"Project {project} name is not unique.");

            if (!configuration.IgnoreConsistency && projects.Any(p => p.Version != project.Version))
                throw new InvalidOperationException($"Project {project} uses different version {project.Version} than others.");

            return project;
        }
    }
}