using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Tasks;
using Xs.Tasks.Dependencies;

namespace Xs.Commands
{
    internal class DeleteCommand : Command<DeleteCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "delete";
        public override string Description { get; } = "Delete dependency from projects.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly DeletePackageDependencyTask deletePackageDependencyTask;
        private readonly DeleteProjectDependencyTask deleteProjectDependencyTask;
        private readonly ILogger<DeleteCommand> logger;

        public DeleteCommand(
            DiscoverProjectsTask discoverTask,
            DeletePackageDependencyTask deletePackageDependencyTask,
            DeleteProjectDependencyTask deleteProjectDependencyTask,
            ILogger<DeleteCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.deletePackageDependencyTask = deletePackageDependencyTask;
            this.deleteProjectDependencyTask = deleteProjectDependencyTask;
            this.logger = logger;
        }

        public override void Handle(
            DeleteCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var name = cfg.Dependency;

            var allProjects = discoverTask.Run(discoverCfg);
            var allPackages = allProjects.SelectMany(e => e.Packages).Select(d => d.Value).Distinct().ToArray();

            var targets = allProjects.FilterMask(cfg.Mask).ToArray();
            if (targets.Length == 0)
            {
                logger.Info($"No projects found to add dependency to.");
                return;
            }

            logger.Debug($"Try delete dependency {name} from {targets.Length} projects.");

            var projects = allProjects.FilterMask(name).ToArray();
            if (projects.Length > 0)
            {
                foreach (var project in projects)
                    deleteProjectDependencyTask.Run(targets.FilterType(project.Type).ToArray(), project);

                return;
            }

            var packages = allPackages.FilterMask(name).Distinct().ToArray();

            // if no packages found
            if (packages.Length == 0)
            {
                logger.Info($"Dependency {name} is neither project nor project dependency. Nothing to do.");
                return;
            }

            foreach (var package in packages)
                deletePackageDependencyTask.Run(targets.FilterType(package.Type).ToArray(), package);
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

        [Position(3, isRequired : false)]
        [Help("Dependency type.")]
        public DependencyType Type { get; set; } = DependencyType.Normal;
    }
}