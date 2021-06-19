using System.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tasks.Dependencies
{
    public class DeleteProjectDependencyTask : ILogSubject
    {
        public ILogger Logger { get; }

        public DeleteProjectDependencyTask(
            ILogger<DeleteProjectDependencyTask> logger
        )
        {
            Logger = logger;
        }

        public void Run(IProject[] targets, IProject project)
        {
            this.Log().Debug($"Delete project {project} as {project.Type} dependency from {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (!target.Projects.Any(p => p.Value == project))
                {
                    this.Log().Debug($"Skip deleting project {project} as dependency of {target}. {target} doesn't use {project}.");
                    continue;
                }

                this.Log().Debug($"Delete project {project} from dependencies of {target}.");
                target.Projects.RemoveWhere(p => p.Value == project);
                target.Save();
            }
        }
    }
}