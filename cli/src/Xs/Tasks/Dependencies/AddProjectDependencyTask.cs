using System.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Tasks.Dependencies
{
    internal class AddProjectDependencyTask
    {
        private readonly ILogger<AddProjectDependencyTask> logger;

        public AddProjectDependencyTask(
            ILogger<AddProjectDependencyTask> logger
        )
        {
            this.logger = logger;
        }

        public void Run(IProject[] targets, Dependency<IProject> dependency)
        {
            var(_, project) = dependency;

            logger.Debug($"Add project {project} as {project.Type} dependency to {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (target.Projects.Contains(dependency))
                {
                    logger.Debug($"Skip adding project {project} as dependency of {target}. {target} already uses {project}.");
                    continue;
                }

                if (target.Projects.Any(p => p.Value == project))
                {
                    logger.Debug($"Delete project {project} as dependency of {target} due to dependency type change.");
                    target.Projects.RemoveWhere(p => p.Value == project);
                }

                logger.Debug($"Add project {project} as dependency of {target}.");
                target.Projects.Add(dependency);
                target.Save();
            }
        }
    }
}