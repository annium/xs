using System.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tasks.Dependencies
{
    public class AddPackageDependencyTask
    {
        private readonly ILogger<AddPackageDependencyTask> _logger;

        public AddPackageDependencyTask(
            ILogger<AddPackageDependencyTask> logger
        )
        {
            _logger = logger;
        }

        public void Run(IProject[] targets, Dependency<Package> dependency)
        {
            var(_, package) = dependency;

            _logger.Debug($"Add package {package} as {package.Type} dependency to {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (target.Packages.Contains(dependency))
                {
                    _logger.Debug($"Skip adding package {package} as dependency of {target}. {target} already uses {package}.");
                    continue;
                }

                if (target.Packages.Any(p => p.Value == package))
                {
                    _logger.Debug($"Delete package {package} as dependency of {target} due to dependency type change.");
                    target.Packages.RemoveWhere(p => p.Value == package);
                }

                _logger.Debug($"Add package {package} as dependency of {target}.");
                target.Packages.Add(dependency);
                target.Save();
            }
        }
    }
}