using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tasks;
using Xs.Tools;

namespace Xs.Commands
{
    internal class UpdateCommand : AsyncCommand<UpdateCommandConfiguration, DiscoverConfiguration>, ILogSubject
    {
        public override string Id => "update";
        public override string Description => "Update dependencies in projects.";
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
            ILogger<UpdateCommand> logger
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
            var projects = _discoverTask.RunAsync(discoverCfg).Await()
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();
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

            this.Debug($"Update {dependencies.Length} dependencies in {projects.Length} projects.");

            // resolve dependency managers for types
            var dependencyManagers = dependencies
                .Select(d => d.Type)
                .Distinct()
                .ToDictionary(
                    t => t,
                    t => _dependencyManagers.SingleOrDefault(m => m.Type == t) ??
                        throw new InvalidOperationException($"No dependency manager registered for {t} dependencies")
                );

            // resolve configuration and available version of all dependencies
            var configuration = _configurationManager.Load(discoverCfg.Root);

            var updates = (await Task.WhenAll(dependencies.Select(async d =>
            {
                var dependencyManager = dependencyManagers[d.Type];
                var registryUri = configuration.Servers.FirstOrDefault(s => s.Key == d.Type).Value;
                var versions = registryUri != null && !registryUri.IsFile ? await dependencyManager.ResolveVersionsAsync(d, registryUri, configuration!.Token) : Array.Empty<Package>();

                // fallback to default server result
                if (versions.Length == 0)
                    versions = await dependencyManager.ResolveVersionsAsync(d, dependencyManager.DefaultServer, string.Empty);

                var result = cfg.Preview ? versions.FirstOrDefault() : versions.FirstOrDefault(v => v.Version.Suffix == "");
                this.Trace($"Resolve: {d} - {versions.Length} version(s)");

                if (result is null)
                    this.Warn($"Resolve: {d} unresolved");
                else if (result == d)
                    this.Debug($"Resolve: {d} unchanged");
                else
                    this.Debug($"Resolve: {d} -> {result}");

                return result;
            }))).OfType<Package>().ToArray();

            if (cfg.DryRun)
            {
                foreach (var project in projects)
                    if (UpdateProject(project, updates))
                        this.Info($"{project} is to be updated.");

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
            this.Debug($"Clear {updated.Count} projects cache.");
            await _runner.RunAsync(
                updated.OfType<ICachingProject>(),
                (project, tkn) => project.ClearCacheAsync(tkn),
                false,
                ct
            );

            this.Debug($"Install {updated.Count} projects.");
            await _runner.RunAsync(
                updated.OfType<IInstallableProject>(),
                (project, tkn) => project.InstallAsync(true, tkn),
                false,
                ct
            );

            this.Info($"{updated.Count} projects updated.");
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
    }
}