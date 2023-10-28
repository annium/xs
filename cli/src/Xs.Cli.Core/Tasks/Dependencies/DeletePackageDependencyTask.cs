using System.Linq;
using Annium.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tasks.Dependencies;

public class DeletePackageDependencyTask : ILogSubject
{
    public ILogger Logger { get; }

    public DeletePackageDependencyTask(ILogger logger)
    {
        Logger = logger;
    }

    public void Run(IProject[] targets, Package package)
    {
        this.Debug($"Delete package {package} as {package.Type} dependency from {targets.Length} projects.");
        foreach (var target in targets)
        {
            if (target.Packages.All(p => p.Value != package))
            {
                this.Debug(
                    $"Skip deleting package {package} as dependency of {target}. {target} doesn't use {package}."
                );
                continue;
            }

            this.Debug($"Delete package {package} from dependencies of {target}.");
            target.Packages.RemoveWhere(p => p.Value == package);
            target.Save();
        }
    }
}
