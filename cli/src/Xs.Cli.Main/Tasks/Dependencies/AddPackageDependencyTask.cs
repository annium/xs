using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Main.Tasks.Dependencies
{
    internal class AddPackageDependencyTask
    {
        private readonly ILogger logger;

        public AddPackageDependencyTask(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public void Run(IProject[] targets, Dependency package)
        {
            logger.Debug($"Add package {package} as {package.Type} dependency to {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (target.PackageDependencies.Contains(package))
                {
                    logger.Debug($"Skip adding package {package} as dependency of {target}. {target} already uses {package}.");
                    continue;
                }

                logger.Debug($"Add package {package} as dependency of {target}.");
                target.PackageDependencies.Add(package);
                target.Save();
            }
        }
    }
}