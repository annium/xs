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
            var projects = candidates
                .Select(x => CreateProject(x.Key, x.Value, configuration, errors.Add)!)
                .Where(x => x != null)
                .ToList();
            var directories = projects.Select(x => x.Directory).ToHashSet();
            var result = projects.OrderBy(e => e.Name).ToArray();
            ThrowIfAnyErrors();

            foreach (var project in result)
                CollectProjects(
                    directories,
                    project,
                    configuration,
                    x =>
                    {
                        projects.Add(x);
                        directories.Add(x.Directory);
                    },
                    errors.Add
                );

            var types = projects.Select(p => p.Type).Distinct().ToArray();

            var packages = types.ToDictionary(type => type, type => new HashSet<Package>());
            LinkProjects(projects, packages, configuration, errors.Add, ThrowIfAnyErrors);
            ThrowIfAnyErrors();

            _logger.Debug($"Discovery finished. Found {projects.Count} projects.");

            return result;

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

        private void CollectProjects(
            IReadOnlyCollection<string> directories,
            IProject project,
            DiscoverConfiguration configuration,
            Action<IProject> addProject,
            Action<Exception> addError
        )
        {
            _logger.Debug($"Discover {project} referenced projects.");
            var lookupDirectories = project.Projects.Select(x => x.Value.Directory).ToArray();

            foreach (var directory in lookupDirectories)
            {
                // project may have been already discovered
                if (directories.Contains(directory))
                    continue;

                var factory = _projectFactory.ResolveFactory(directory);
                if (factory is null)
                {
                    addError(new InvalidOperationException($"Can't find factory for project in {directory}"));
                    continue;
                }

                var dependency = CreateProject(directory, factory, configuration, addError);
                if (dependency != null)
                {
                    addProject(dependency);
                    CollectProjects(directories, dependency, configuration, addProject, addError);
                }
            }
        }

        private IProject? CreateProject(
            string directory,
            ISpecialProjectFactory factory,
            DiscoverConfiguration configuration,
            Action<Exception> addError
        )
        {
            try
            {
                var project = factory.CreateProject(directory, configuration);
                _logger.Debug($"{project.Type} {project} created at {directory}");
                return project;
            }
            catch (Exception exception)
            {
                addError(exception);
                return null;
            }
        }

        private void LinkProjects(
            IReadOnlyCollection<IProject> projects,
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