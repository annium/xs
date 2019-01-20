using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tasks.Dependencies
{
    internal class DeletePackageDependencyTask
    {
        private readonly ILogger logger;

        public DeletePackageDependencyTask(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public void Run(IProject[] targets, Dependency package)
        {
            logger.LogDebug($"Delete package {package} as {package.Type} dependency from {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (!target.PackageDependencies.Contains(package))
                {
                    logger.LogDebug($"Skip deleting package {package} as dependency of {target}. {target} doesn't use {package}.");
                    continue;
                }

                logger.LogDebug($"Delete package {package} from dependencies of {target}.");
                target.PackageDependencies.Remove(package);
                target.Save();
            }
        }
    }
}