using System.Linq;
using Annium.Logging;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;

namespace Annium.Xs.Cli.Core.Tasks.Dependencies;

public class DeletePackageDependencyTask : ILogSubject
{
    public ILogger Logger { get; }

    public DeletePackageDependencyTask(ILogger logger)
    {
        Logger = logger;
    }

    public void Run(IProject[] targets, Package package)
    {
        this.Debug(
            "Delete package {package} as {packageType} dependency from {targetsLength} projects.",
            package,
            package.Type,
            targets.Length
        );
        foreach (var target in targets)
        {
            if (target.Packages.All(p => p.Value != package))
            {
                this.Debug(
                    "Skip deleting package {package} as dependency of {target}. {target} doesn't use {package}.",
                    package,
                    target,
                    target,
                    package
                );
                continue;
            }

            this.Debug("Delete package {package} from dependencies of {target}.", package, target);
            target.Packages.RemoveWhere(p => p.Value == package);
            target.Save();
        }
    }
}
