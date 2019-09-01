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
    internal class UseCommand : Command<UseCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "use";
        public override string Description { get; } = "Set dependency in projects to specific version.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly AddPackageDependencyTask addPackageDependencyTask;
        private readonly DeletePackageDependencyTask deletePackageDependencyTask;
        private readonly ILogger<UseCommand> logger;

        public UseCommand(
            DiscoverProjectsTask discoverTask,
            AddPackageDependencyTask addPackageDependencyTask,
            DeletePackageDependencyTask deletePackageDependencyTask,
            ILogger<UseCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.addPackageDependencyTask = addPackageDependencyTask;
            this.deletePackageDependencyTask = deletePackageDependencyTask;
            this.logger = logger;
        }

        public override void Handle(
            UseCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var name = cfg.Name;
            var version = cfg.Version;

            var allProjects = discoverTask.Run(discoverCfg);
            var updatedPackages = allProjects
                .SelectMany(e => e.Packages)
                .FilterMask(name)
                .Where(e => e.Value.Version != version)
                .Distinct()
                .ToArray();

            var targets = allProjects
                .Where(e => e.Packages.Any(d => updatedPackages.Contains(d)))
                .ToArray();

            if (targets.Length == 0)
            {
                logger.Info($"No projects found to update.");
                return;
            }

            foreach (var old in updatedPackages)
            {
                var dependency = new Dependency<Package>(old.Type, new Package(old.Value.Type, old.Value.Name, version));
                var subset = targets.FilterType(dependency.Value.Type).ToArray();
                deletePackageDependencyTask.Run(subset, old.Value);
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