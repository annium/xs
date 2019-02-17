using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tasks.Dependencies;

namespace Xs.Cli.Main.Commands
{
    internal class UseCommand : Command<UseCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "use";

        public override string Description { get; } = "Set dependency in projects to specific version.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectTypeTask filterTypeTask;

        private readonly AddPackageDependencyTask addPackageDependencyTask;

        private readonly DeletePackageDependencyTask deletePackageDependencyTask;

        private readonly ILogger logger;

        public UseCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectTypeTask filterTypeTask,
            AddPackageDependencyTask addPackageDependencyTask,
            DeletePackageDependencyTask deletePackageDependencyTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.filterTypeTask = filterTypeTask;
            this.addPackageDependencyTask = addPackageDependencyTask;
            this.deletePackageDependencyTask = deletePackageDependencyTask;
            this.logger = logger;
        }

        public override void Handle(
            UseCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var nameLow = cfg.Name.ToLowerInvariant();
            var version = cfg.Version;

            var allProjects = discoverTask.Run(cwdCfg.Cwd);
            var updatedDependencies = allProjects
                .SelectMany(e => e.PackageDependencies)
                .Where(e => e.Name.ToLowerInvariant() == nameLow && e.Version != version)
                .Distinct()
                .ToArray();

            var targets = allProjects
                .Where(e => e.PackageDependencies.Any(d => updatedDependencies.Contains(d)))
                .ToArray();

            if (targets.Length == 0)
            {
                logger.LogInfo($"No projects found to update.");
                return;
            }

            foreach (var old in updatedDependencies)
            {
                var dependency = new Dependency(old.Type, old.Name, version);
                var subset = filterTypeTask.Run(targets, dependency.Type);
                deletePackageDependencyTask.Run(subset, old);
                addPackageDependencyTask.Run(subset, dependency);
            }
        }
    }

    internal class UseCommandConfiguration
    {
        [Position(1)]
        [Help("Dependency name.")]
        public string Name { get; set; }

        [Position(2)]
        [Help("Dependency version.")]
        public Version Version { get; set; }
    }
}