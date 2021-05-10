using System.Linq;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands
{
    internal class UnlinkCommand : Command<UnlinkCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "unlink";
        public override string Description { get; } = "Unlink project <-> package dependencies.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ILogger<UseCommand> _logger;

        public UnlinkCommand(
            DiscoverProjectsTask discoverTask,
            ILogger<UseCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _logger = logger;
        }

        public override void Handle(
            UnlinkCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            discoverCfg.Roots = new[] { cfg.Target };
            var targets = _discoverTask.RunAsync(discoverCfg).Await().ToArray();
            discoverCfg.Roots = new[] { cfg.Source };
            var sources = _discoverTask.RunAsync(discoverCfg).Await()
                .Where(x => targets.All(t => t.File != x.File))
                .ToArray();
            var version = cfg.Version;

            _logger.Debug($"Unlink {sources.Length} projects from {targets.Length} external projects.");

            foreach (var source in sources)
            {
                var externalDependencies = source.Projects
                    .Where(x => targets.Any(t => t.File == x.Value.File))
                    .ToList();
                if (externalDependencies.Count == 0)
                    continue;

                foreach (var project in externalDependencies)
                {
                    var package = new Package(project.Value.Type, project.Value.Name, version);
                    _logger.Trace($"Update {source}: replace {project} with {package}.");

                    source.Projects.Remove(project);
                    source.Packages.Add(new Dependency<Package>(project.Type, package));
                }

                _logger.Debug($"Updated {source}.");

                source.Save();
            }
        }
    }

    internal class UnlinkCommandConfiguration
    {
        [Position(1)]
        [Help("Source projects root, that will receive links.")]
        public string Source { get; set; } = string.Empty;

        [Position(2)]
        [Help("Target projects root, containing projects' source projects' will be linked to.")]
        public string Target { get; set; } = string.Empty;

        [Position(3)]
        [Help("Dependency version, when switching to packages.")]
        public Version Version { get; set; } = Version.Empty;
    }
}