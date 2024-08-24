using System.Linq;
using Annium.Logging;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;

namespace Xx.Cli.Core.Tasks.Dependencies;

public class AddPackageDependencyTask : ILogSubject
{
    public ILogger Logger { get; }

    public AddPackageDependencyTask(ILogger logger)
    {
        Logger = logger;
    }

    public void Run(IProject[] targets, Dependency<Package> dependency)
    {
        var (_, package) = dependency;

        this.Debug($"Add package {package} as {package.Type} dependency to {targets.Length} projects.");
        foreach (var target in targets)
        {
            if (target.Packages.Contains(dependency))
            {
                this.Debug(
                    $"Skip adding package {package} as dependency of {target}. {target} already uses {package}."
                );
                continue;
            }

            if (target.Packages.Any(p => p.Value == package))
            {
                this.Debug($"Delete package {package} as dependency of {target} due to dependency type change.");
                target.Packages.RemoveWhere(p => p.Value == package);
            }

            this.Debug($"Add package {package} as dependency of {target}.");
            target.Packages.Add(dependency);
            target.Save();
        }
    }
}
