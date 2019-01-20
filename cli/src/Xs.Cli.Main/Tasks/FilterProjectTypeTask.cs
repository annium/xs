using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tasks
{
    public class FilterProjectTypeTask
    {
        private readonly ILogger logger;

        public FilterProjectTypeTask(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public IProject[] Run(IEnumerable<IProject> projects, ProjectType type)
        {
            var result = projects.Where(e => e.Type == type).ToArray();

            logger.LogDebug($"Filtered {result.Length} of {projects.Count()} projects by type {type}.");

            return result;
        }

        public Dependency[] Run(IEnumerable<Dependency> dependencies, ProjectType type)
        {
            var result = dependencies.Where(e => e.Type == type).ToArray();

            logger.LogDebug($"Filtered {result.Length} of {dependencies.Count()} dependencies by type {type}.");

            return result;
        }
    }
}