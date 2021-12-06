using System.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tasks.Dependencies;

public class AddProjectDependencyTask : ILogSubject
{
    public ILogger Logger { get; }

    public AddProjectDependencyTask(
        ILogger<AddProjectDependencyTask> logger
    )
    {
        Logger = logger;
    }

    public void Run(IProject[] targets, Dependency<IProject> dependency)
    {
        var (_, project) = dependency;

        this.Log().Debug($"Add project {project} as {project.Type} dependency to {targets.Length} projects.");
        foreach (var target in targets)
        {
            if (target.Projects.Contains(dependency))
            {
                this.Log().Debug($"Skip adding project {project} as dependency of {target}. {target} already uses {project}.");
                continue;
            }

            if (target.Projects.Any(p => p.Value == project))
            {
                this.Log().Debug($"Delete project {project} as dependency of {target} due to dependency type change.");
                target.Projects.RemoveWhere(p => p.Value == project);
            }

            this.Log().Debug($"Add project {project} as dependency of {target}.");
            target.Projects.Add(dependency);
            target.Save();
        }
    }
}