using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Tasks;
using Xs.Cli.Core.Tasks.Dependencies;

namespace Xs.Commands;

internal class DeleteCommand
    : AsyncCommand<DeleteCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "delete";
    public static string Description => "Delete dependency from projects.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly DeletePackageDependencyTask _deletePackageDependencyTask;
    private readonly DeleteProjectDependencyTask _deleteProjectDependencyTask;

    public DeleteCommand(
        DiscoverProjectsTask discoverTask,
        DeletePackageDependencyTask deletePackageDependencyTask,
        DeleteProjectDependencyTask deleteProjectDependencyTask,
        ILogger logger
    )
    {
        _discoverTask = discoverTask;
        _deletePackageDependencyTask = deletePackageDependencyTask;
        _deleteProjectDependencyTask = deleteProjectDependencyTask;
        Logger = logger;
    }

    public override async Task HandleAsync(
        DeleteCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var name = cfg.Dependency;

        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var allPackages = allProjects.SelectMany(e => e.Packages).Select(d => d.Value).Distinct().ToArray();

        var targets = allProjects.FilterMask(cfg.Mask).ToArray();
        if (targets.Length == 0)
        {
            this.Info($"No projects found to add dependency to.");
            return;
        }

        this.Debug($"Try delete dependency {name} from {targets.Length} projects.");

        var projects = allProjects.FilterMask(name).ToArray();
        if (projects.Length > 0)
        {
            foreach (var project in projects)
                _deleteProjectDependencyTask.Run(targets.FilterType(project.Type).ToArray(), project);

            return;
        }

        var packages = allPackages.FilterMask(name).Distinct().ToArray();

        // if no packages found
        if (packages.Length == 0)
        {
            this.Info($"Dependency {name} is neither project nor project dependency. Nothing to do.");
            return;
        }

        foreach (var package in packages)
            _deletePackageDependencyTask.Run(targets.FilterType(package.Type).ToArray(), package);
    }
}

internal class DeleteCommandConfiguration
{
    [Position(1)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = string.Empty;

    [Position(2)]
    [Help("Dependency.")]
    public string Dependency { get; set; } = string.Empty;

    [Position(3, isRequired: false)]
    [Help("Dependency type.")]
    public DependencyType Type { get; set; } = DependencyType.Normal;
}
