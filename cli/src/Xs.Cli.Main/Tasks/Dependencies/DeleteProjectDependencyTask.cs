using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tasks.Dependencies
{
    internal class DeleteProjectDependencyTask
    {
        private readonly ILogger logger;

        public DeleteProjectDependencyTask(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public void Run(IProject[] targets, IProject project)
        {
            logger.LogDebug($"Delete project {project} as {project.Type} dependency from {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (!target.ProjectDependencies.Contains(project))
                {
                    logger.LogDebug($"Skip deleting project {project} as dependency of {target}. {target} doesn't use {project}.");
                    continue;
                }

                logger.LogDebug($"Delete project {project} from dependencies of {target}.");
                target.ProjectDependencies.Remove(project);
                target.Save();
            }
        }
    }
}