using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tasks.Dependencies;

namespace Xs.Cli.Main.Commands
{
    internal class DeleteCommand : AsyncCommand<DeleteCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "delete";

        public override string Description { get; } = "Delete dependency from projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        private readonly FilterProjectTypeTask filterTypeTask;

        private readonly DeletePackageDependencyTask deletePackageDependencyTask;

        private readonly DeleteProjectDependencyTask deleteProjectDependencyTask;

        private readonly ILogger logger;

        public DeleteCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask,
            FilterProjectTypeTask filterTypeTask,
            DeletePackageDependencyTask deletePackageDependencyTask,
            DeleteProjectDependencyTask deleteProjectDependencyTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
            this.filterTypeTask = filterTypeTask;
            this.deletePackageDependencyTask = deletePackageDependencyTask;
            this.deleteProjectDependencyTask = deleteProjectDependencyTask;
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
                logger.LogInfo($"No projects found to add dependency to.");
                return;
            }

            logger.LogDebug($"Try delete dependency {name} from {targets.Length} projects.");

            var projects = allProjects.Where(e => e.Name.ToLowerInvariant() == nameLow).ToArray();
            if (projects.Length > 0)
            {
                foreach (var project in projects)
                    deleteProjectDependencyTask.Run(filterTypeTask.Run(targets, project.Type), project);

                return;
            }

            var packages = dependencies.Where(e => e.Name.ToLowerInvariant() == nameLow).Distinct().ToArray();

            // if no packages found
            if (packages.Length == 0)
            {
                logger.LogInfo($"Dependency {name} is neither project nor project dependency. Nothing to do.");
                return;
            }

            foreach (var package in packages)
                deletePackageDependencyTask.Run(filterTypeTask.Run(targets, package.Type), package);
        }
    }

    internal class DeleteCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask.")]
        public string Mask { get; set; }

        [Position(2)]
        [Help("Dependency.")]
        public string Dependency { get; set; }
    }
}