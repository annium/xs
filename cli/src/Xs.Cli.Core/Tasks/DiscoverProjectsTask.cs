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
        private readonly IProjectFactory _projectFactory;
        private readonly IProjectLinker _projectLinker;
        private readonly ILogger<DiscoverProjectsTask> _logger;

        public DiscoverProjectsTask(
            IProjectFactory projectFactory,
            IProjectLinker projectLinker,
            ILogger<DiscoverProjectsTask> logger
        )
        {
            _projectFactory = projectFactory;
            _projectLinker = projectLinker;
            _logger = logger;
        }

        public IEnumerable<IProject> Run(DiscoverConfiguration configuration)
        {
            var roots = configuration.Roots;

            _logger.Debug($"Start discovery of {string.Join(", ", roots)}.");

            var candidates = FindProjectCandidates(roots);

            var errors = new List<Exception>();

            var projects = CreateProjects(candidates, configuration, errors.Add);
            ThrowIfAnyErrors();

            var types = projects.Select(p => p.Type).Distinct().ToArray();

            var packages = types.ToDictionary(type => type, type => new HashSet<Package>());
            LinkProjects(projects, packages, configuration, errors.Add, ThrowIfAnyErrors);
            ThrowIfAnyErrors();

            _logger.Debug($"Discovery finished. Found {projects.Count} projects.");

            return projects.OrderBy(e => e.Name).ToArray();

            void ThrowIfAnyErrors()
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
                _logger.Debug($"Start project candidates lookup at {root}.");

                FileManager.WalkDirectories(
                    root,
                    directory =>
                    {
                        var factory = _projectFactory.ResolveFactory(directory);
                        if (factory is null)
                            return false;

                        results[directory] = factory;
                        _logger.Debug($"{factory.Type} project candidate discovered at {directory}.");

                        return true;
                    },
                    SearchOptions.IgnoreChildrenOnMatch
                );

                _logger.Debug($"{results.Count} project candidate(s) found.");
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

            _logger.Debug("Start projects creation.");

            foreach (var (directory, factory) in candidates)
            {
                try
                {
                    var project = factory.CreateProject(directory, configuration);
                    projects.Add(project);
                    _logger.Debug($"{project.Type} {project} created at {directory}");
                }
                catch (Exception exception)
                {
                    addError(exception);
                }
            }

            _logger.Debug($"{projects.Count} project(s) created.");

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
            _logger.Debug("Start projects linking.");

            _projectLinker.PreLink(projects, packages, configuration, addError);

            throwIfAnyErrors();

            foreach (var project in projects)
            {
                var typePackages = packages[project.Type];
                _projectLinker.Link(project, projects, typePackages, configuration, addError);
            }

            _logger.Debug("Projects linked.");
        }
    }
}