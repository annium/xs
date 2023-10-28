using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;

namespace Xs.Commands;

internal class LinkCommand
    : AsyncCommand<LinkCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "link";
    public static string Description => "Link project <-> package dependencies.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;

    public LinkCommand(DiscoverProjectsTask discoverTask, ILogger logger)
    {
        _discoverTask = discoverTask;
        Logger = logger;
    }

    public override async Task HandleAsync(
        LinkCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        discoverCfg.Roots = new[] { cfg.Source };
        var sources = await _discoverTask.RunAsync(discoverCfg);
        discoverCfg.Roots = new[] { cfg.Target };
        var targets = await _discoverTask.RunAsync(discoverCfg);

        this.Debug($"Link {sources.Count} projects to {targets.Count} external projects.");

        foreach (var src in sources)
        {
            var externalDependencies = src.Packages
                .Select(x =>
                {
                    var nameLower = x.Value.Name.ToLowerInvariant();

                    return (package: x, project: targets.FirstOrDefault(t => t.Name.ToLowerInvariant() == nameLower));
                })
                .Where(x => x.project is not null)
                .ToList();
            if (externalDependencies.Count == 0)
                continue;

            foreach (var (package, project) in externalDependencies)
            {
                // otherwise - it's external and it's reference is converted to project
                this.Trace($"Update {src}: replace {package} with {project}.");

                src.Packages.Remove(package);
                src.Projects.Add(new Dependency<IProject>(package.Type, project!));
            }

            this.Debug($"Updated {src}.");

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
