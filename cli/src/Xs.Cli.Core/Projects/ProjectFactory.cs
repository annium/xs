using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Commands;

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
            DiscoverConfiguration configuration
        ) => factory.CreateProject(
            directory,
            configuration
        );
    }
}