using System.Linq;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tasks.Dependencies;

namespace Xs.Commands
{
    internal class UseCommand : Command<UseCommandConfiguration, DiscoverConfiguration>, ILogSubject
    {
        public override string Id => "use";
        public override string Description => "Set dependency in projects to specific version.";
        public ILogger Logger { get; }
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly AddPackageDependencyTask _addPackageDependencyTask;
        private readonly DeletePackageDependencyTask _deletePackageDependencyTask;

        public UseCommand(
            DiscoverProjectsTask discoverTask,
            AddPackageDependencyTask addPackageDependencyTask,
            DeletePackageDependencyTask deletePackageDependencyTask,
            ILogger<UseCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _addPackageDependencyTask = addPackageDependencyTask;
            _deletePackageDependencyTask = deletePackageDependencyTask;
            Logger = logger;
        }

        public override void Handle(
            UseCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            var name = cfg.Name;
            var version = cfg.Version;

            var allProjects = _discoverTask.RunAsync(discoverCfg);
            var updatedPackages = allProjects.Await()
                .SelectMany(e => e.Packages)
                .FilterMask(name)
                .Where(e => e.Value.Version != version)
                .Distinct()
                .ToArray();

            var targets = allProjects.Await()
                .Where(e => e.Packages.Any(d => updatedPackages.Contains(d)))
                .ToArray();

            if (targets.Length == 0)
            {
                this.Log().Info($"No projects found to update.");
                return;
            }

            foreach (var old in updatedPackages)
            {
                var dependency = new Dependency<Package>(old.Type, new Package(old.Value.Type, old.Value.Name, version));
                var subset = targets.FilterType(dependency.Value.Type).ToArray();
                _deletePackageDependencyTask.Run(subset, old.Value);
                _addPackageDependencyTask.Run(subset, dependency);
            }
        }
    }

    internal class UseCommandConfiguration
    {
        [Position(1)]
        [Help("Dependency name.")]
        public string Name { get; set; } = string.Empty;

        [Position(2)]
        [Help("Dependency version.")]
        public Version Version { get; set; } = Version.Empty;
    }
}