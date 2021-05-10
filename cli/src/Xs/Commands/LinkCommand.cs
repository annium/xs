using System.Linq;
using System.Threading;
using Annium.Core.Primitives;
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
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly ILogger<UseCommand> _logger;

        public LinkCommand(
            DiscoverProjectsTask discoverTask,
            ILogger<UseCommand> logger
        )
        {
            _discoverTask = discoverTask;
            _logger = logger;
        }

        public override void Handle(
            LinkCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken ct
        )
        {
            discoverCfg.Roots = new[] { cfg.Source };
            var sources = _discoverTask.RunAsync(discoverCfg).Await().ToArray();
            discoverCfg.Roots = new[] { cfg.Target };
            var targets = _discoverTask.RunAsync(discoverCfg).Await().ToArray();

            _logger.Debug($"Link {sources.Length} projects to {targets.Length} external projects.");

            foreach (var src in sources)
            {
                var externalDependencies = src.Packages
                    .Select(x =>
                    {
                        var nameLower = x.Value.Name.ToLowerInvariant();

                        return (package: x, project: targets.FirstOrDefault(t => t.Name.ToLowerInvariant() == nameLower));
                    })
                    .Where(x => x.project != null)
                    .ToList();
                if (externalDependencies.Count == 0)
                    continue;

                foreach (var (package, project) in externalDependencies)
                {
                    // otherwise - it's external and it's reference is converted to project
                    _logger.Trace($"Update {src}: replace {package} with {project}.");

                    src.Packages.Remove(package);
                    src.Projects.Add(new Dependency<IProject>(package.Type, project));
                }

                _logger.Debug($"Updated {src}.");

                src.Save();
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