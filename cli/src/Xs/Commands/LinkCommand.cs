using System.Linq;
using System.Threading;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands
{
    internal class LinkCommand : Command<LinkCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "link";
        public override string Description { get; } = "Link project <-> package dependencies.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ILogger<UseCommand> logger;

        public LinkCommand(
            DiscoverProjectsTask discoverTask,
            ILogger<UseCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.logger = logger;
        }

        public override void Handle(
            LinkCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            discoverCfg.Roots = new[] { cfg.Source };
            var sources = discoverTask.Run(discoverCfg).ToArray();
            discoverCfg.Roots = new[] { cfg.Target };
            var targets = discoverTask.Run(discoverCfg).ToArray();

            logger.Debug($"Link {sources.Length} projects to {targets.Length} external projects.");

            foreach (var src in sources)
            {
                var changed = false;

                // check all package dependencies
                foreach (var dependency in src.Packages.ToArray())
                {
                    // if dependency is not in projects - no action 
                    var nameLower = dependency.Value.Name.ToLowerInvariant();
                    var target = targets.FirstOrDefault(p => p.Name.ToLowerInvariant() == nameLower);
                    if (target is null)
                        continue;

                    // otherwise - it's external and it's reference is converted to project
                    logger.Trace($"Update {src}: replace {dependency} with {target}.");

                    src.Packages.Remove(dependency);
                    src.Projects.Add(new Dependency<IProject>(dependency.Type, target));
                    changed = true;
                }

                if (changed)
                {
                    logger.Debug($"Updated {src}.");

                    src.Save();
                }
            }
        }
    }

    internal class LinkCommandConfiguration
    {
        [Position(1)]
        [Help("Source projects root, that will receive links.")]
        public string Source { get; set; } = string.Empty;

        [Position(2)]
        [Help("Target projects root, containing projects' source projects' will be linked to.")]
        public string Target { get; set; } = string.Empty;
    }
}