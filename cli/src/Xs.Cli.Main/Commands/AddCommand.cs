using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Main.Tasks;

namespace Xs.Cli.Main.Commands
{
    internal class AddCommand : AsyncCommand<AddCommandConfiguration, CwdCommandConfiguration>
    {
        public override string Id { get; } = "add";

        public override string Description { get; } = "add dependency to project.";

        private readonly DiscoverProjectsTask discoverTask;

        private readonly FilterProjectsTask filterTask;

        private readonly ILogger logger;

        public AddCommand(
            DiscoverProjectsTask discoverTask,
            FilterProjectsTask filterTask,
            ILogger logger
        )
        {
            this.discoverTask = discoverTask;
            this.filterTask = filterTask;
            this.logger = logger;
        }

        public override async Task HandleAsync(
            AddCommandConfiguration cfg,
            CwdCommandConfiguration cwdCfg,
            CancellationToken token
        )
        {
            var name = cfg.Dependency.ToLowerInvariant();
            var version = cfg.Version;

            var allProjects = await discoverTask.RunAsync(cwdCfg.Cwd);
            var dependencies = allProjects.SelectMany(e => e.PackageDependencies).Distinct().ToArray();

            var targets = filterTask.Run(allProjects, cfg.Mask).ToArray();
            if (targets.Length == 0)
            {
                logger.LogInfo($"No projects found to add dependency to.");
                return;
            }

            logger.LogDebug($"Try add dependency {name} to {targets.Length} projects.");

            var projects = allProjects.Where(e => e.Name.ToLowerInvariant() == name).ToArray();
            if (projects.Length > 0)
            {
                foreach (var project in projects)
                    AddProjectDependency(
                        targets.Where(e => e.Type == project.Type && !e.ProjectDependencies.Contains(project)).ToArray(),
                        project
                    );

                return;
            }

            logger.LogDebug($"Assume dependency {name} as package.");
            var packages = ProjectType.List().ToDictionary(
                e => e,
                e => dependencies.FirstOrDefault(d => d.Type == e && d.Name.ToLowerInvariant() == name)
            );

            // if at least one package not found
            if (packages.Values.Any(e => e == null))
            {
                // if no version - can't define dependency
                if (version == null)
                    throw new InvalidOperationException($"Package {name} is not used for yet in some projects. Specify version to use.");
                // else - new dependencies is added
                else
                    packages = packages.ToDictionary(e => e.Key, e => new Dependency(e.Key, name, version));
            }

            // if package already exists: if version exists - check it's same, otherwise - nothing to do.
            else if (version != null && packages.Values.Any(e => e.Version != version))
                throw new ArgumentException($"Package {name} is already used with different version. Specify already used version, or narrow projects mask.");

            foreach (var package in packages.Values)
                AddPackageDependency(
                    targets.Where(e => e.Type == package.Type && !e.PackageDependencies.Contains(package)).ToArray(),
                    package
                );
        }

        private void AddProjectDependency(IProject[] targets, IProject project)
        {
            logger.LogDebug($"Resolved to project {project}. Add it to {targets.Length} projects.");
            foreach (var target in targets)
            {
                logger.LogDebug($"Add project {project} as dependency of {target}.");
                target.ProjectDependencies.Add(project);
                target.Save();
            }
        }

        private void AddPackageDependency(IProject[] targets, Dependency package)
        {
            logger.LogDebug($"Resolved to package {package}. Add it to {targets.Length} projects.");
            foreach (var target in targets)
            {
                logger.LogDebug($"Add package {package} as dependency of {target}.");
                target.PackageDependencies.Add(package);
                target.Save();
            }
        }
    }

    internal class AddCommandConfiguration
    {
        [Position(1)]
        [Help("Projects mask.")]
        public string Mask { get; set; }

        [Position(2)]
        [Help("Dependency.")]
        public string Dependency { get; set; }

        [Position(3, isRequired : false)]
        [Help("Dependency version (for package dependencies).")]
        public Core.Models.Version Version { get; set; }
    }
}