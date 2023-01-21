using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Commands;
using Xs.Cli.Core.Models;

namespace Xs.Cli.Core.Projects;

internal class ProjectLinker : IProjectLinker
{
    private readonly IEnumerable<ISpecialProjectLinker> _linkers;

    public ProjectLinker(IEnumerable<ISpecialProjectLinker> linkers)
    {
        _linkers = linkers;
    }

    public void PreLink(
        IReadOnlyCollection<IProject> projects,
        IReadOnlyDictionary<ProjectType, HashSet<Package>> packages,
        DiscoverConfiguration configuration,
        Action<Exception> addError
    )
    {
        if (!configuration.IgnoreConsistency && projects.Select(p => p.Version.ToString()).Distinct().Count() > 1)
            addError(new InvalidOperationException(
                $"Projects use multiple versions:{Environment.NewLine}{string.Join(Environment.NewLine, projects.Select(p => $"{p}: {p.Version}"))}."
            ));

        var projectsByTypes = projects.GroupBy(p => p.Type).ToDictionary(g => g.Key, g => g.ToArray());
        foreach (var (type, typeProjects) in projectsByTypes)
        {
            var linker = _linkers.FirstOrDefault(l => l.Type == type);
            if (linker is null)
            {
                addError(new InvalidOperationException($"No linker found for project type {type}"));
                continue;
            }

            foreach (var package in typeProjects.SelectMany(p => p.Packages).Select(d => d.Value))
                packages[type].Add(package);

            if (!configuration.IgnoreConsistency)
                ValidatePackages(typeProjects, packages[type], addError);

            linker.PreLink(
                typeProjects,
                configuration,
                addError
            );
        }
    }

    public void Link(
        IProject project,
        IReadOnlyCollection<IProject> projects,
        IReadOnlyCollection<Package> packages,
        DiscoverConfiguration configuration,
        Action<Exception> addError
    )
    {
        var duplicateProject = projects.FirstOrDefault(p => p != project && p.Name == project.Name);
        if (duplicateProject is not null)
        {
            addError(new InvalidOperationException($"Project {project} name is not unique."));
            return;
        }

        if (configuration.ForceChecks)
        {
            var duplicatePackage = packages.FirstOrDefault(p => p.Name == project.Name);
            if (!(duplicatePackage is null))
            {
                addError(new InvalidOperationException($"Project {project} name is used by package {duplicatePackage}."));
                return;
            }
        }

        var linker = _linkers.First(l => l.Type == project.Type);
        linker.Link(
            project,
            projects,
            packages,
            configuration,
            addError
        );
    }

    private void ValidatePackages(
        IReadOnlyCollection<IProject> projects,
        IReadOnlyCollection<Package> packages,
        Action<Exception> addError
    )
    {
        foreach (var group in packages.GroupBy(p => p.Name.ToLowerInvariant()))
        {
            var name = group.Key;
            var variations = group.ToArray();
            if (variations.Length == 1)
                continue;

            var usages = projects
                .Select(p => (
                    project: p,
                    package: p.Packages.FirstOrDefault(d => d.Value.Name.ToLowerInvariant() == name)?.Value
                ))
                .Where(p => p.package is not null)
                .ToArray();
            var variationsString = string.Join(
                Environment.NewLine,
                usages.Select(p => $"- {p.project}: {p.package}")
            );
            addError(new InvalidOperationException($"Package {name} is used in different variations:{Environment.NewLine}{variationsString}"));
        }
    }
}