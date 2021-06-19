using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Core.Tasks
{
    public class DiscoverProjectsTask : ILogSubject
    {
        public ILogger Logger { get; }
        private readonly IProjectFactory _projectFactory;
        private readonly IProjectLinker _projectLinker;
        private readonly IShell _shell;

        public DiscoverProjectsTask(
            IProjectFactory projectFactory,
            IProjectLinker projectLinker,
            IShell shell,
            ILogger<DiscoverProjectsTask> logger
        )
        {
            _projectFactory = projectFactory;
            _projectLinker = projectLinker;
            _shell = shell;
            Logger = logger;
        }

        public async Task<IReadOnlyCollection<IProject>> RunAsync(DiscoverConfiguration configuration)
        {
            var roots = configuration.Roots;

            this.Log().Debug($"Start discovery of {string.Join(", ", roots)}.");

            var candidates = FindProjectCandidates(roots);
            var errors = new List<Exception>();
            var projects = candidates
                .Select(x => CreateProject(x.Key, x.Value, configuration, errors.Add)!)
                .Where(x => x != null!)
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

            var packages = types.ToDictionary(type => type, _ => new HashSet<Package>());
            LinkProjects(projects, packages, configuration, errors.Add, ThrowIfAnyErrors);
            ThrowIfAnyErrors();

            this.Log().Debug($"Discovery finished. Found {projects.Count} projects.");

            if (!configuration.Changed)
                return result;

            // filter project with changed files only.
            this.Log().Debug($"Discovery finished. Found {projects.Count} projects.");
            var changes = await new DiscoverChangedFilesTask(_shell).RunAsync(roots);

            var filteredProjects = result.Where(x => changes.Any(c => c.Contains(x.Directory))).ToArray();

            return filteredProjects;

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
                this.Log().Debug($"Start project candidates lookup at {root}.");

                FileManager.WalkDirectories(
                    root,
                    directory =>
                    {
                        var factory = _projectFactory.ResolveFactory(directory);
                        if (factory is null)
                            return false;

                        results[directory] = factory;
                        this.Log().Debug($"{factory.Type} project candidate discovered at {directory}.");

                        return true;
                    },
                    SearchOptions.IgnoreChildrenOnMatch
                );

                this.Log().Debug($"{results.Count} project candidate(s) found.");
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
            this.Log().Debug($"Discover {project} referenced projects.");
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
                this.Log().Debug($"{project.Type} {project} created at {directory}");
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
            this.Log().Debug("Start projects linking.");

            _projectLinker.PreLink(projects, packages, configuration, addError);

            throwIfAnyErrors();

            foreach (var project in projects)
            {
                var typePackages = packages[project.Type];
                _projectLinker.Link(project, projects, typePackages, configuration, addError);
            }

            this.Log().Debug("Projects linked.");
        }
    }
}