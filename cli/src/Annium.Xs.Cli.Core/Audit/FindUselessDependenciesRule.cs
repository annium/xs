using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;

namespace Annium.Xs.Cli.Core.Audit;

public class FindUselessDependenciesRule<TProject> : IAuditRule<TProject>
    where TProject : IProject
{
    public string Code => "useless-deps";
    public string Description => "Finds useless dependencies in projects. Fix deletes useless deps";

    public IReadOnlyCollection<AuditResult> Execute(IReadOnlyCollection<IProject> projects, TProject project, bool fix)
    {
        var results = new List<AuditResult>();

        // check project dependencies
        foreach (
            var dependency in project
                .Projects.Where(d => d.Type != DependencyType.Dev && d.Type != DependencyType.Peer)
                .ToArray()
        )
        {
            var foundDependencies = FindProjectDependenciesDeep(project, p => p.Projects.Contains(dependency));
            if (foundDependencies.Length == 0)
                continue;

            if (fix)
                project.Projects.Remove(dependency);

            results.Add(
                new AuditResult(
                    fix,
                    $"Useless project {dependency} reference, already used by: {string.Join(", ", foundDependencies.Select(e => e.Name))}"
                )
            );
        }

        // check package dependencies
        foreach (
            var dependency in project
                .Packages.Where(d => d.Type != DependencyType.Dev && d.Type != DependencyType.Peer)
                .ToArray()
        )
        {
            var foundDependencies = FindProjectDependenciesDeep(project, p => p.Packages.Contains(dependency));
            if (foundDependencies.Length == 0)
                continue;

            if (fix)
                project.Packages.Remove(dependency);

            results.Add(
                new AuditResult(
                    fix,
                    $"Useless package {dependency} reference, already used by: {string.Join(", ", foundDependencies.Select(e => e.Name))}"
                )
            );
        }

        if (fix && results.Count > 0)
            project.Save();

        return results;
    }

    private IProject[] FindProjectDependenciesDeep(IProject project, Func<IProject, bool> isMatch)
    {
        if (project.Projects.Count == 0)
            return [];

        var matches = new List<IProject>();
        foreach (var dependency in project.Projects)
        {
            if (isMatch(dependency.Value))
                matches.Add(dependency.Value);

            var nestedMatches = FindProjectDependenciesDeep(dependency.Value, isMatch);
            if (nestedMatches.Length > 0)
                matches.AddRange(nestedMatches);
        }

        return matches.Distinct().ToArray();
    }
}
