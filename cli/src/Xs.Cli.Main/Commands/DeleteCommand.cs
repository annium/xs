using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tasks.Dependencies;

namespace Xs.Cli.Main.Commands
{
    internal class DeleteCommand : Command<DeleteCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "delete";

        public override string Description { get; } = "Delete dependency from projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly DeletePackageDependencyTask deletePackageDependencyTask;

        private readonly DeleteProjectDependencyTask deleteProjectDependencyTask;

        private readonly ILogger logger;

        public DeleteCommand(
            DiscoverProjectsTask discoverTask,
            DeletePackageDependencyTask deletePackageDependencyTask,
            DeleteProjectDependencyTask deleteProjectDependencyTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.deletePackageDependencyTask = deletePackageDependencyTask;
            this.deleteProjectDependencyTask = deleteProjectDependencyTask;
            this.logger = logger;
        }

        public override void Handle(
            DeleteCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var name = cfg.Dependency;
            var nameLow = name.ToLowerInvariant();

            var allProjects = discoverTask.Run(cwdCfg.Cwd);
            var dependencies = allProjects.SelectMany(e => e.PackageDependencies).Distinct().ToArray();

            var targets = allProjects.FilterMask(cfg.Mask).ToArray();
            if (targets.Length == 0)
            {
                logger.Info($"No projects found to add dependency to.");
                return;
            }

            logger.Debug($"Try delete dependency {name} from {targets.Length} projects.");

            var projects = allProjects.Where(e => e.Name.ToLowerInvariant() == nameLow).ToArray();
            if (projects.Length > 0)
            {
                foreach (var project in projects)
                    deleteProjectDependencyTask.Run(targets.FilterType(project.Type).ToArray(), project);

                return;
            }

            var packages = dependencies.Where(e => e.Name.ToLowerInvariant() == nameLow).Distinct().ToArray();

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
    }
}