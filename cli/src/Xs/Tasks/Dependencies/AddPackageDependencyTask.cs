using System.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Tasks.Dependencies
{
    internal class AddPackageDependencyTask
    {
        private readonly ILogger<AddPackageDependencyTask> logger;

        public AddPackageDependencyTask(
            ILogger<AddPackageDependencyTask> logger
        )
        {
            this.logger = logger;
        }

        public void Run(IProject[] targets, Dependency<Package> dependency)
        {
            var(_, package) = dependency;

            logger.Debug($"Add package {package} as {package.Type} dependency to {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (target.Packages.Contains(dependency))
                {
                    logger.Debug($"Skip adding package {package} as dependency of {target}. {target} already uses {package}.");
                    continue;
                }

                if (target.Packages.Any(p => p.Value == package))
                {
                    logger.Debug($"Delete package {package} as dependency of {target} due to dependency type change.");
                    target.Packages.RemoveWhere(p => p.Value == package);
                }

                logger.Debug($"Add package {package} as dependency of {target}.");
                target.Packages.Add(dependency);
                target.Save();
            }
        }
    }
}