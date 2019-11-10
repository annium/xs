using System.Linq;
using System.Threading;
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
        public override string Description { get; } = "Link project <-> package dependencies.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ILogger<UseCommand> logger;

        public UnlinkCommand(
            DiscoverProjectsTask discoverTask,
            ILogger<UseCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.logger = logger;
        }

        public override void Handle(
            UnlinkCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            discoverCfg.Roots = new[] { cfg.Source, cfg.Target };
            var sources = discoverTask.Run(discoverCfg).ToArray();
            discoverCfg.Roots = new[] { cfg.Target };
            var targets = discoverTask.Run(discoverCfg).ToArray();
            var version = cfg.Version;

            logger.Debug($"Link {sources.Length} projects to {targets.Length} external projects.");

            foreach (var source in sources)
            {
                var changed = false;

                // check all project dependencies
                foreach (var dependency in source.Projects.ToArray())
                {
                    // if dependency is in sources - no action 
                    if (sources.Contains(dependency.Value))
                        continue;

                    // otherwise - it's external and it's reference is converted to package
                    var package = new Package(dependency.Value.Type, dependency.Value.Name, version);
                    logger.Trace($"Update {source}: replace {dependency} with {package}.");

                    source.Projects.Remove(dependency);
                    source.Packages.Add(new Dependency<Package>(dependency.Type, package));
                    changed = true;
                }

                if (changed)
                {
                    logger.Debug($"Updated {source}.");

                    source.Save();
                }
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

        [Position(3, isRequired: false)]
        [Help("Dependency version, when switching to packages.")]
        public Version Version { get; set; } = Version.Empty;
    }
}