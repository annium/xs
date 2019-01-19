using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands
{
    internal class DeleteCommand : AsyncCommand<DeleteCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "delete";

        public override string Description { get; } = "delete dependency from project";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        private readonly ILogger logger;

        public DeleteCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            DeleteCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var name = cfg.Dependency;
            var nameLow = name.ToLowerInvariant();

            var allProjects = await discoverTask.RunAsync(cwdCfg.Cwd);
            var dependencies = allProjects.SelectMany(e => e.PackageDependencies).Distinct().ToArray();

            var targets = filterTask.Run(allProjects, cfg.Mask).ToArray();
            if (targets.Length == 0)
            {
                logger.LogInfo($"No projects found to add dependency to");
                return;
            }

            logger.LogDebug($"Try delete dependency {name} from {targets.Length} projects");

            var projects = allProjects.Where(e => e.Name.ToLowerInvariant() == nameLow).ToArray();
            if (projects.Length > 0)
            {
                foreach (var project in projects)
                    DeleteProjectDependency(
                        targets.Where(e => e.Type == project.Type && e.ProjectDependencies.Contains(project)).ToArray(),
                        project
                    );

                return;
            }

            var packages = dependencies.Where(e => e.Name.ToLowerInvariant() == nameLow).Distinct().ToArray();

            // if no packages found
            if (packages.Length == 0)
            {
                logger.LogInfo($"Dependency {name} is neither project nor project dependency. Nothing to do");
                return;
            }

            foreach (var package in packages)
                DeletePackageDependency(
                    targets.Where(e => e.Type == package.Type && e.PackageDependencies.Contains(package)).ToArray(),
                    package
                );
        }

        private void DeleteProjectDependency(IProject[] targets, IProject project)
        {
            logger.LogDebug($"Resolved to project {project}. Delete it from {targets.Length} projects.");
            foreach (var target in targets)
            {
                logger.LogDebug($"Delete project {project} from dependencies of {target}.");
                target.ProjectDependencies.Remove(project);
                target.Save();
            }
        }

        private void DeletePackageDependency(IProject[] targets, Dependency package)
        {
            logger.LogDebug($"Resolved to package {package}. Delete it from {targets.Length} projects");
            foreach (var target in targets)
            {
                logger.LogDebug($"Delete package {package} from dependencies of {target}.");
                target.PackageDependencies.Remove(package);
                target.Save();
            }
        }
    }

    internal class DeleteCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask")]
        public string Mask { get; set; }

        [Position(2)]
        [Help("Dependency")]
        public string Dependency { get; set; }
    }
}