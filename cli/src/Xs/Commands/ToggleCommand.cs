using System.Collections.Generic;
using System.IO;
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
    internal class ToggleCommand : Command<ToggleCommandConfiguration, DiscoverConfiguration>
    {
        public override string Id { get; } = "toggle";
        public override string Description { get; } = "Toggle project <-> package dependencies.";
        private readonly DiscoverProjectsTask discoverTask;
        private readonly ILogger<UseCommand> logger;

        public ToggleCommand(
            DiscoverProjectsTask discoverTask,
            ILogger<UseCommand> logger
        )
        {
            this.discoverTask = discoverTask;
            this.logger = logger;
        }

        public override void Handle(
            ToggleCommandConfiguration cfg,
            DiscoverConfiguration discoverCfg,
            CancellationToken token
        )
        {
            var directory = Path.GetFullPath(cfg.Directory);

            var allProjects = discoverTask.Run(discoverCfg)
                .FilterMask(cfg.Mask)
                .FilterType(cfg.Type)
                .ToArray();
            var targets = allProjects
                .Where(p => p.Directory.StartsWith(directory))
                .Distinct()
                .ToArray();

            if (targets.Length == 0)
            {
                logger.Info($"No projects found to toggle.");
                return;
            }

            switch (cfg.Action)
            {
                case ToggleCommandAction.Package:
                    ToggleProjectsToPackages(targets, cfg.Version);
                    break;
                case ToggleCommandAction.Project:
                    TogglePackagesToProjects(targets, allProjects);
                    break;
            }
        }

        private void ToggleProjectsToPackages(
            IEnumerable<IProject> targets,
            Version version
        )
        {
            logger.Debug($"Toggle {targets.Count()} projects to use external projects as packages.");

            foreach (var target in targets)
            {
                var changed = false;

                // check all project dependencies
                foreach (var dependency in target.Projects.ToArray())
                {
                    // if dependency is in targets - no action 
                    if (targets.Contains(dependency.Value))
                        continue;

                    // otherwise - it's external and it's reference is converted to package
                    var package = new Package(dependency.Value.Type, dependency.Value.Name, version);
                    logger.Trace($"Update {target}: replace {dependency} with {package}.");

                    target.Projects.Remove(dependency);
                    target.Packages.Add(new Dependency<Package>(dependency.Type, package));
                    changed = true;
                }

                if (changed)
                {
                    logger.Debug($"Updated {target}.");

                    target.Save();
                }
            }
        }

        private void TogglePackagesToProjects(
            IEnumerable<IProject> targets,
            IEnumerable<IProject> projects
        )
        {
            logger.Debug($"Toggle {targets.Count()} projects to use packages as external projects.");

            foreach (var target in targets)
            {
                var changed = false;

                // check all package dependencies
                foreach (var dependency in target.Packages.ToArray())
                {
                    // if dependency is not in projects - no action 
                    var nameLower = dependency.Value.Name.ToLowerInvariant();
                    var project = projects.FirstOrDefault(p => p.Name.ToLowerInvariant() == nameLower);
                    if (project is null)
                        continue;

                    // otherwise - it's external and it's reference is converted to project
                    logger.Trace($"Update {target}: replace {dependency} with {project}.");

                    target.Packages.Remove(dependency);
                    target.Projects.Add(new Dependency<IProject>(dependency.Type, project));
                    changed = true;
                }

                if (changed)
                {
                    logger.Debug($"Updated {target}.");

                    target.Save();
                }
            }
        }
    }

    internal class ToggleCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask.")]
        public string Mask { get; set; } = "all";

        [Position(2)]
        [Help("Project type.")]
        public ProjectType Type { get; set; } = ProjectType.None;

        [Position(3)]
        [Help("Toggle command action.")]
        public ToggleCommandAction Action { get; set; }

        [Position(4)]
        [Help("Directory to toggle projects within.")]
        public string Directory { get; set; } = string.Empty;

        [Position(5, isRequired : false)]
        [Help("Dependency version, when switching to packages.")]
        public Version Version { get; set; } = Version.Empty;
    }

    internal enum ToggleCommandAction
    {
        Package,
        Project
    }
}