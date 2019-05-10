using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;
using Xs.Cli.Main.Tools;

namespace Xs.Cli.Main.Commands
{
    internal class UpdateCommand : AsyncCommand<UpdateCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "update";

        public override string Description { get; } = "Update dependencies in projects.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly IEnumerable<IDependencyManager> dependencyManagers;

        private readonly IConfigurationManager configurationManager;

        private readonly ProjectsRunner runner;

        private readonly ILogger logger;

        public UpdateCommand(
            DiscoverProjectsTask discoverTask,
            IEnumerable<IDependencyManager> dependencyManagers,
            IConfigurationManager configurationManager,
            ProjectsRunner runner,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.dependencyManagers = dependencyManagers;
            this.configurationManager = configurationManager;
            this.runner = runner;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            UpdateCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var projects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();
            if (projects.Length == 0)
            {
                logger.Info($"No projects found to update.");
                return;
            }

            var dependencies = projects.SelectMany(e => e.Packages).Select(e => e.Value).Distinct().ToArray();
            if (dependencies.Length == 0)
            {
                logger.Info($"No projects found to update.");
                return;
            }
            logger.Debug($"Update {dependencies.Length} dependencies in {projects.Length} projects.");

            // resolve dependency managers for types
            var dependencyManagers = dependencies
                .Select(d => d.Type)
                .Distinct()
                .ToDictionary(
                    t => t,
                    t => this.dependencyManagers.FirstOrDefault(m => m.Type == t) ??
                    throw new InvalidOperationException($"No dependency manager registered for {t} dependencies")
                );

            // resolve configuration and available version of all dependencies
            var configuration = await configurationManager.LoadAsync(discoverCfg.Root);
            if (configuration == null)
            {
                logger.Warn($"No registry tracked at {discoverCfg.Root}. Track registry first.");
                return;
            }

            var updates = (await Task.WhenAll(dependencies.Select(async d =>
            {
                var versions = await dependencyManagers[d.Type].GetVersionsAsync(d, configuration);
                var result = cfg.Preview ? versions.FirstOrDefault() : versions.FirstOrDefault(v => v.Version.Suffix == null);
                logger.Debug($"Resolve: {d} -> {result}");

                return result;
            }))).OfType<Package>().ToArray();

            if (cfg.DryRun)
            {
                foreach (var project in projects)
                    if (UpdateProject(project, updates))
                        logger.Info($"{project} is to be updated.");

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
                logger.Info($"No projects updated.");
                return;
            }

            // install installable updates
            logger.Debug($"Clear {updated.Count} projects cache.");
            await runner.RunAsync(
                updated.OfType<ICachingProject>(),
                (project, tkn) => project.ClearCacheAsync(tkn),
                token);

            logger.Debug($"Install {updated.Count} projects.");
            await runner.RunAsync(
                updated.OfType<IInstallableProject>(),
                (project, tkn) => project.InstallAsync(true, tkn),
                token);

            logger.Info($"{updated.Count} projects updated.");
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
                if (update == null || (update.Name == d.Name && update.Version <= d.Version))
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
        [Position(1, isRequired : false)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2, isRequired : false)]
        [Help("Project type.")]
        public ProjectType Type { get; set; }

        [Option(isRequired: false)]
        [Help("Allow suffixed.")]
        public bool Preview { get; set; }

        [Option("dry", isRequired : false)]
        [Help("Dry run.")]
        public bool DryRun { get; set; }
    }
}