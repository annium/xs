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
    internal class UseCommand : AsyncCommand<UseCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "use";

        public override string Description { get; } = "set global dependency to specific version";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly ILogger logger;

        public UseCommand(
            DiscoverProjectsTask discoverTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            UseCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var name = cfg.Name.ToLowerInvariant();
            var version = cfg.Version;

            var allProjects = await discoverTask.RunAsync(cwdCfg.Cwd);
            var updatedDependencies = allProjects
                .SelectMany(e => e.PackageDependencies)
                .Where(e => e.Name.ToLowerInvariant() == name && e.Version != version)
                .Distinct()
                .ToArray();

            var targets = allProjects
                .Where(e => e.PackageDependencies.Any(d => updatedDependencies.Contains(d)))
                .ToArray();

            if (targets.Length == 0)
            {
                logger.LogInfo($"No projects found to update");
                return;
            }

            foreach (var dependency in updatedDependencies.Select(d => new Dependency(d.Type, d.Name, version)))
                UsePackageDependency(targets.Where(e => e.Type == dependency.Type).ToArray(), dependency);
        }

        private void UsePackageDependency(IProject[] targets, Dependency dependency)
        {
            logger.LogDebug($"Use {dependency} in {targets.Length} projects");
            foreach (var target in targets)
            {
                var current = target.PackageDependencies.First(e => e.Name == dependency.Name);
                logger.LogDebug($"Use in {target}: {current} -> {dependency}");
                target.PackageDependencies.Remove(current);
                target.PackageDependencies.Add(dependency);
                target.Save();
            }
        }
    }

    internal class UseCommandConfiguration
    {
        [Position(1)]
        [Help("Dependency name")]
        public string Name { get; set; }

        [Position(2)]
        [Help("Dependency version")]
        public Version Version { get; set; }
    }
}