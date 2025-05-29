using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Core.Tasks.Dependencies;

namespace Annium.Xs.Cli.Commands;

internal class UseCommand
    : AsyncCommand<UseCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "use";
    public static string Description => "Set dependency in projects to specific version.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly AddPackageDependencyTask _addPackageDependencyTask;
    private readonly DeletePackageDependencyTask _deletePackageDependencyTask;

    public UseCommand(
        DiscoverProjectsTask discoverTask,
        AddPackageDependencyTask addPackageDependencyTask,
        DeletePackageDependencyTask deletePackageDependencyTask,
        ILogger logger
    )
    {
        _discoverTask = discoverTask;
        _addPackageDependencyTask = addPackageDependencyTask;
        _deletePackageDependencyTask = deletePackageDependencyTask;
        Logger = logger;
    }

    public override async Task HandleAsync(
        UseCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var name = cfg.Name;
        var version = cfg.Version;

        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var updatedPackages = allProjects
            .SelectMany(e => e.Packages)
            .FilterMask(name)
            .Where(e => !e.Value.Version.Equals(version))
            .Distinct()
            .ToArray();

        var targets = allProjects.Where(e => e.Packages.Any(d => updatedPackages.Contains(d))).ToArray();

        if (targets.Length == 0)
        {
            this.Info("No projects found to update.");
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
