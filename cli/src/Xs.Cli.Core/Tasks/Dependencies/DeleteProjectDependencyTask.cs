using System.Linq;
using Annium.Logging;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tasks.Dependencies;

public class DeleteProjectDependencyTask : ILogSubject
{
    public ILogger Logger { get; }

    public DeleteProjectDependencyTask(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public void Run(IProject[] targets, IProject project)
    {
        this.Debug($"Delete project {project} as {project.Type} dependency from {targets.Length} projects.");
        foreach (var target in targets)
        {
            if (target.Projects.All(p => p.Value != project))
            {
                this.Debug($"Skip deleting project {project} as dependency of {target}. {target} doesn't use {project}.");
                continue;
            }

            this.Debug($"Delete project {project} from dependencies of {target}.");
            target.Projects.RemoveWhere(p => p.Value == project);
            target.Save();
        }
    }
}