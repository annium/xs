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
using Xs.Cli.Core.Tasks.Dependencies;
using Xs.Tools;

namespace Xs.Commands
{
    internal class AddCommand : AsyncCommand<AddCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "add";
        public override string Description { get; } = "Add dependency to projects.";
        private readonly DiscoverProjectsTask _discoverTask;
        private readonly IEnumerable<IDependencyManager> _dependencyManagers;
        private readonly IConfigurationManager _configurationManager;
        private readonly AddPackageDependencyTask _addPackageDependencyTask;
        private readonly AddProjectDependencyTask _addProjectDependencyTask;
        private readonly ILogger<AddCommand> _logger;

        public AddCommand(
            DiscoverProjectsTask discoverTask,
            IEnumerable<IDependencyManager> dependencyManagers,
            IConfigurationManager configurationManager,
            AddPackageDependencyTask addPackageDependencyTask,
            AddProjectDependencyTask addProjectDependencyTask,
            ILogger<AddCommand> logger
        )
        {
            _addPackageDependencyTask = addPackageDependencyTask;
            _addProjectDependencyTask = addProjectDependencyTask;
            _discoverTask = discoverTask;
            _dependencyManagers = dependencyManagers;
            _configurationManager = configurationManager;
            _logger = logger;
        }

        public override async Task HandleAsync(
            AddCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var name = cfg.Name;
            var version = cfg.Version;
            var dependencyType = cfg.DependencyType;

            var allProjects = _discoverTask.RunAsync(discoverCfg).Await().ToArray();

            var targets = allProjects.FilterMask(cfg.Mask).ToArray();
            if (targets.Length == 0)
            {
                _logger.Info("No projects found to add dependency to.");
                return;
            }

            var projectType = ResolveProjectType(targets, cfg.Mask);
            var allPackages = allProjects
                .Where(x => x.Type == projectType)
                .SelectMany(e => e.Packages)
                .Select(d => d.Value)
                .Distinct()
                .ToArray();

            var projects = allProjects.FilterMask(name).Except(targets).ToArray();
            if (projects.Length > 0)
            {
                foreach (var project in projects)
                {
                    _logger.Debug($"Add '{projectType}' project dependency '{name}' to {targets.Length} projects.");
                    _addProjectDependencyTask.Run(targets, new Dependency<IProject>(dependencyType, project));
                }

                return;
            }

            _logger.Debug($"Assume dependency {name} is package.");
            var packages = allPackages.FilterMask(name).ToArray();

            // if no packages match name and no version given - resolve
            if (packages.Length == 0)
                packages = new[] { await ResolvePackage(discoverCfg, cfg, projectType, name, version) };

            // if package already exists: if version exists - check it's same, otherwise - nothing to do.
            else if (version != Cli.Core.Models.Version.Empty)
                EnsureNoVersionConflict(packages, version);

            foreach (var package in packages)
                _addPackageDependencyTask.Run(targets.FilterType(package.Type).ToArray(), new Dependency<Package>(dependencyType, package));
        }

        private ProjectType ResolveProjectType(IProject[] targets, string mask)
        {
            var targetGroups = targets.GroupBy(x => x.Type).ToDictionary(x => x.Key, x => x.ToArray());
            if (targetGroups.Count > 1)
            {
                var targetsErrorView = string.Join(
                    Environment.NewLine,
                    targetGroups.Select(x => string.Join(
                        Environment.NewLine,
                        $"{x.Key}:",
                        string.Join(Environment.NewLine, x.Value.Select(p => $" - {p}"))
                    ))
                );
                throw new InvalidOperationException(
                    $"Projects mask '{mask}' matches projects of different types:{Environment.NewLine}{targetsErrorView}"
                );
            }

            return targetGroups.Single().Key;
        }

        private void EnsureNoVersionConflict(Package[] packages, Cli.Core.Models.Version version)
        {
            if (packages.All(x => x.Version == version))
                return;

            var conflictsErrorView = string.Join(
                Environment.NewLine,
                packages.Where(x => x.Version != version).Select(x => $" - {x}: {x.Version}")
            );
            throw new ArgumentException($"Package {packages.First().Name} is already used with different version:{Environment.NewLine}{conflictsErrorView}");
        }

        private async Task<Package> ResolvePackage(
            DiscoverConfiguration discoverCfg,
            AddCommandConfiguration cfg,
            ProjectType projectType,
            string name,
            Cli.Core.Models.Version version
        )
        {
            if (version != Cli.Core.Models.Version.Empty)
                return new Package(projectType, name, version);

            var packageStub = new Package(projectType, name, Cli.Core.Models.Version.Empty);

            // resolve configuration and available version of all dependencies
            var configuration = _configurationManager.Load(discoverCfg.Root);

            var dependencyManager = _dependencyManagers.Single(x => x.Type == packageStub.Type);


            var registryUri = configuration?.Servers.FirstOrDefault(s => s.Key == packageStub.Type).Value;
            var versions = registryUri != null && !registryUri.IsFile
                ? await dependencyManager.ResolveVersionsAsync(packageStub, registryUri, configuration!.Token)
                : Array.Empty<Package>();

            // fallback to default server result
            if (versions.Length == 0)
                versions = await dependencyManager.ResolveVersionsAsync(packageStub, dependencyManager.DefaultServer, string.Empty);

            var package = cfg.Preview ? versions.FirstOrDefault() : versions.FirstOrDefault(v => v.Version.Suffix == "");
            _logger.Trace($"Resolve: {packageStub} - {versions.Length} version(s)");

            if (package is null)
                throw new InvalidOperationException($"Resolve: {packageStub} unresolved");

            _logger.Debug($"Resolve: {packageStub} -> {package}");

            return package;
        }
    }

    internal class AddCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = string.Empty;

        [Position(2)]
        [Help("Dependency name.")]
        public string Name { get; set; } = string.Empty;

        [Position(3, isRequired: false)]
        [Help("Dependency version (for package dependencies, optional).")]
        public Cli.Core.Models.Version Version { get; set; } = Cli.Core.Models.Version.Empty;

        [Option("t")]
        [Help("Dependency type.")]
        public DependencyType DependencyType { get; set; } = DependencyType.Normal;

        [Option("p")]
        [Help("Allow suffixed.")]
        public bool Preview { get; set; } = false;
    }
}