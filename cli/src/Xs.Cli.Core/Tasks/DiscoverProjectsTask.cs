using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Tasks
{
    public class DiscoverProjectsTask
    {
        private readonly IProjectFactory projectFactory;
        private readonly IProjectLinker projectLinker;
        private readonly ILogger<DiscoverProjectsTask> logger;

        public DiscoverProjectsTask(
            IProjectFactory projectFactory,
            IProjectLinker projectLinker,
            ILogger<DiscoverProjectsTask> logger
        )
        {
            this.projectFactory = projectFactory;
            this.projectLinker = projectLinker;
            this.logger = logger;
        }

        public IEnumerable<IProject> Run(DiscoverConfiguration configuration)
        {
            var roots = configuration.Roots;

            logger.Debug($"Start discovery of {string.Join(", ", roots)}.");

            var candidates = FindProjectCandidates(roots);

            var errors = new List<Exception>();

            var projects = CreateProjects(candidates, configuration, errors.Add);
            throwIfAnyErrors();

            var types = projects.Select(p => p.Type).Distinct().ToArray();

            var packages = types.ToDictionary(type => type, type => new HashSet<Package>());
            LinkProjects(projects, packages, configuration, errors.Add, throwIfAnyErrors);
            throwIfAnyErrors();

            logger.Debug($"Discovery finished. Found {projects.Count} projects.");

            return projects.OrderBy(e => e.Name).ToArray();

            void throwIfAnyErrors()
            {
                if (errors.Count > 0)
                    throw new AggregateException(errors);
            }
        }

        private IReadOnlyDictionary<string, ISpecialProjectFactory> FindProjectCandidates(IReadOnlyCollection<string> roots)
        {
            var results = new Dictionary<string, ISpecialProjectFactory>();

            foreach (var root in roots)
            {
                logger.Debug($"Start project candidates lookup at {root}.");

                FileManager.WalkDirectories(
                    root,
                    directory =>
                    {
                        var factory = projectFactory.FindFactory(directory);
                        if (factory is null)
                            return false;

                        results[directory] = factory;
                        logger.Debug($"{factory.Type} project candidate discovered at {directory}.");

                        return true;
                    },
                    SearchOptions.IgnoreChildrenOnMatch
                );

                logger.Debug($"{results.Count} project candidate(s) found.");
            }

            return results;
        }

        private HashSet<IProject> CreateProjects(
            IReadOnlyDictionary<string, ISpecialProjectFactory> candidates,
            DiscoverConfiguration configuration,
            Action<Exception> addError
        )
        {
            var projects = new HashSet<IProject>();

            logger.Debug("Start projects creation.");

            foreach (var (directory, factory) in candidates)
            {
                try
                {
                    var project = projectFactory.CreateProject(directory, factory, configuration);
                    projects.Add(project);
                    logger.Debug($"{project.Type} {project} created at {directory}");
                }
                catch (Exception exception)
                {
                    addError(exception);
                }
            }

            logger.Debug($"{projects.Count} project(s) created.");

            return projects;
        }

        private void LinkProjects(
            HashSet<IProject> projects,
            IReadOnlyDictionary<ProjectType, HashSet<Package>> packages,
            DiscoverConfiguration configuration,
            Action<Exception> addError,
            Action throwIfAnyErrors
        )
        {
            logger.Debug("Start projects linking.");

            projectLinker.PreLink(projects, packages, configuration, addError);

            throwIfAnyErrors();

            foreach (var project in projects)
            {
                var typePackages = packages[project.Type];
                projectLinker.Link(project, projects, typePackages, configuration, addError);
            }

            logger.Debug("Projects linked.");
        }
    }
}