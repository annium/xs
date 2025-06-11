using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments.Attributes;
using Annium.Extensions.Arguments.Commands;
using Annium.Logging;
using Annium.Xs.Cli.Core.Commands;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Core.Tasks;
using Annium.Xs.Cli.Core.Tools;
using Annium.Xs.Cli.Tools;

namespace Annium.Xs.Cli.Commands;

internal class UpdateCommand
    : AsyncCommand<UpdateCommandConfiguration, DiscoverConfiguration>,
        ICommandDescriptor,
        ILogSubject
{
    public static string Id => "update";
    public static string Description => "Update dependencies in projects.";
    public ILogger Logger { get; }
    private readonly DiscoverProjectsTask _discoverTask;
    private readonly IEnumerable<IDependencyManager> _dependencyManagers;
    private readonly IConfigurationManager _configurationManager;
    private readonly ProjectsRunner _runner;

    public UpdateCommand(
        DiscoverProjectsTask discoverTask,
        IEnumerable<IDependencyManager> dependencyManagers,
        IConfigurationManager configurationManager,
        ProjectsRunner runner,
        ILogger logger
    )
    {
        _discoverTask = discoverTask;
        _dependencyManagers = dependencyManagers;
        _configurationManager = configurationManager;
        _runner = runner;
        Logger = logger;
    }

    public override async Task HandleAsync(
        UpdateCommandConfiguration cfg,
        DiscoverConfiguration discoverCfg,
        CancellationToken ct
    )
    {
        var allProjects = await _discoverTask.RunAsync(discoverCfg);
        var projects = allProjects.FilterMask(cfg.Mask).FilterType(cfg.Type).ToArray();
        if (projects.Length == 0)
        {
            this.Info("No projects found to update.");
            return;
        }

        var dependencies = projects.SelectMany(e => e.Packages).Select(e => e.Value).Distinct().ToArray();
        if (dependencies.Length == 0)
        {
            this.Info("No projects found to update.");
            return;
        }

        this.Debug(
            "Update {dependenciesLength} dependencies in {projectsLength} projects.",
            dependencies.Length,
            projects.Length
        );

        // resolve dependency managers for types
        var dependencyManagers = dependencies
            .Select(d => d.Type)
            .Distinct()
            .ToDictionary(
                t => t,
                t =>
                    _dependencyManagers.SingleOrDefault(m => m.Type == t)
                    ?? throw new InvalidOperationException($"No dependency manager registered for {t} dependencies")
            );

        // resolve configuration and available version of all dependencies
        var configuration = _configurationManager.Load(discoverCfg.Root);

        var updates = (
            await Task.WhenAll(
                dependencies.Select(async d =>
                {
                    var dependencyManager = dependencyManagers[d.Type];
                    var registryUri = configuration.Servers.GetValueOrDefault(d.Type);
                    var versions =
                        registryUri is not null && !registryUri.IsFile
                            ? await dependencyManager.ResolveVersionsAsync(d, registryUri, configuration.Token)
                            : [];

                    // fallback to default server result
                    if (versions.Length == 0)
                        versions = await dependencyManager.ResolveVersionsAsync(
                            d,
                            dependencyManager.DefaultServer,
                            string.Empty
                        );

                    var result = cfg.Preview
                        ? versions.FirstOrDefault()
                        : versions.FirstOrDefault(v => v.Version.Suffix == "");
                    this.Trace("Resolve: {dependency} - {versionsLength} version(s)", d, versions.Length);

                    if (result is null)
                        this.Warn("Resolve: {dependency} unresolved", d);
                    else if (result == d)
                        this.Debug("Resolve: {dependency} unchanged", d);
                    else
                        this.Debug("Resolve: {dependency} -> {result}", d, result);

                    return result;
                })
            )
        ).OfType<Package>().ToArray();

        if (cfg.DryRun)
        {
            foreach (var project in projects)
                if (UpdateProject(project, updates))
                    this.Debug("{project} is to be updated.", project);

            return;
        }

        // for each project updated - check if it's dependencies is updated, and if yes - update and add to updated list
        var updated = new List<IProject>();
        foreach (var project in projects)
            if (UpdateProject(project, updates))
            {
                project.Save();
                updated.Add(project);
            }

        if (updated.Count == 0)
        {
            this.Info("No projects updated.");
            return;
        }

        // install installable updates
        this.Debug("Clear {updatedCount} projects cache.", updated.Count);
        await _runner.RunAsync(
            updated.OfType<ICachingProject>().ToArray(),
            (project, tkn) => project.ClearCacheAsync(tkn),
            new ProjectsRunner.Config(cfg.Parallelism, false),
            ct
        );

        this.Debug("Install {updatedCount} projects.", updated.Count);
        await _runner.RunAsync(
            updated.OfType<IInstallableProject>().ToArray(),
            (project, tkn) => project.InstallAsync(true, tkn),
            new ProjectsRunner.Config(cfg.Parallelism, false),
            ct
        );

        this.Info("{updatedCount} projects updated.", updated.Count);
    }

    private bool UpdateProject(IProject project, Package[] updates)
    {
        var isUpdated = false;

        foreach (var package in project.Packages.ToList())
        {
            var d = package.Value;
            var name = d.Name.ToLowerInvariant();
            var update = updates.FirstOrDefault(u => u.Type == d.Type && u.Name.ToLowerInvariant() == name);

            // update is not applied if not found, or if naming is same and no newer version is found
            if (update is null || (update.Name == d.Name && update.Version <= d.Version))
                continue;

            project.Packages.Remove(package);
            project.Packages.Add(new Dependency<Package>(package.Type, update));
            isUpdated = true;
        }

        return isUpdated;
    }
}

internal class UpdateCommandConfiguration
{
    [Position(1, isRequired: false)]
    [Help("Projects mask.")]
    public string Mask { get; set; } = "all";

    [Position(2, isRequired: false)]
    [Help("Project type.")]
    public ProjectType Type { get; set; } = ProjectType.None;

    [Option]
    [Help("Allow suffixed.")]
    public bool Preview { get; set; }

    [Option("dry")]
    [Help("Dry run.")]
    public bool DryRun { get; set; }

    [Option("p")]
    [Help("Degree of parallelism. Default - all available tasks are run in parallel")]
    public int Parallelism { get; set; }
}
