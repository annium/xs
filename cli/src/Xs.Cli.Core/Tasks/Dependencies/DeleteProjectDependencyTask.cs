using System.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Tasks.Dependencies
{
    public class DeleteProjectDependencyTask
    {
        private readonly ILogger<DeleteProjectDependencyTask> _logger;

        public DeleteProjectDependencyTask(
            ILogger<DeleteProjectDependencyTask> logger
        )
        {
            _logger = logger;
        }

        public void Run(IProject[] targets, IProject project)
        {
            _logger.Debug($"Delete project {project} as {project.Type} dependency from {targets.Length} projects.");
            foreach (var target in targets)
            {
                if (!target.Projects.Any(p => p.Value == project))
                {
                    _logger.Debug($"Skip deleting project {project} as dependency of {target}. {target} doesn't use {project}.");
                    continue;
                }

                _logger.Debug($"Delete project {project} from dependencies of {target}.");
                target.Projects.RemoveWhere(p => p.Value == project);
                target.Save();
            }
        }
    }
}