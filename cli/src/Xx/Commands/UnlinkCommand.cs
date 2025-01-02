using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xx.Cli.Core.Commands;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Tasks;

namespace Xx.Commands;

internal class UnlinkCommand
    : AsyncCommand<UnlinkCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "unlink";
    public static string Description => "Unlink project <-> package dependencies.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;

    public UnlinkCommand(DiscoverProjectsTask discoverTask, ILogger logger)
    {
        _discoverTask = discoverTask;
        Logger = logger;
    }

    public override async Task HandleAsync(
        UnlinkCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        discoverCfg.Roots = new[] { cfg.Target };
        var targets = await _discoverTask.RunAsync(discoverCfg);
        discoverCfg.Roots = new[] { cfg.Source };
        var sources = (await _discoverTask.RunAsync(discoverCfg))
            .Where(x => targets.All(t => t.File != x.File))
            .ToArray();
        var version = cfg.Version;

        this.Debug(
            "Unlink {sourcesLength} projects from {targetsCount} external projects.",
            sources.Length,
            targets.Count
        );

        foreach (var source in sources)
        {
            var externalDependencies = source.Projects.Where(x => targets.Any(t => t.File == x.Value.File)).ToList();
            if (externalDependencies.Count == 0)
                continue;

            foreach (var project in externalDependencies)
            {
                var package = new Package(project.Value.Type, project.Value.Name, version);
                this.Trace("Update {source}: replace {project} with {package}.", source, project, package);

                source.Projects.Remove(project);
                source.Packages.Add(new Dependency<Package>(project.Type, package));
            }

            this.Debug("Updated {source}.", source);

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
