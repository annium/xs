using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tasks.Dependencies
{
    internal class AddProjectDependencyTask
    {
        private readonly ILogger logger;

        public AddProjectDependencyTask(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public void Run(IProject[] targets, IProject project)
        {
            logger.Debug($"Add project {project} as {project.Type} dependency to {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (target.ProjectDependencies.Contains(project))
                {
                    logger.Debug($"Skip adding project {project} as dependency of {target}. {target} already uses {project}.");
                    continue;
                }

                logger.Debug($"Add project {project} as dependency of {target}.");
                target.ProjectDependencies.Add(project);
                target.Save();
            }
        }
    }
}