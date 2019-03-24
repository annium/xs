using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit
{
    public class FindUselessDependenciesRule<TProject> : IAuditRule<TProject> where TProject : IProject
    {
        public IEnumerable<AuditResult> Execute(IProject[] projects, TProject project, bool fix)
        {
            var results = new List<AuditResult>();

            // check project dependencies
            foreach (var dependency in project.ProjectDependencies.ToArray())
            {
                var foundDependencies = FindProjectDependenciesDeep(project, p => p.ProjectDependencies.Contains(dependency));
                if (foundDependencies.Length == 0)
                    continue;

                if (fix)
                    project.ProjectDependencies.Remove(dependency);

                results.Add(new AuditResult(fix,
                    $"Useless project {dependency} reference, already used by: {string.Join(", ", foundDependencies.Select(e=>e.Name))}"
                ));
            }

            // check package dependencies
            foreach (var dependency in project.PackageDependencies.ToArray())
            {
                var foundDependencies = FindProjectDependenciesDeep(project, p => p.PackageDependencies.Contains(dependency));
                if (foundDependencies.Length == 0)
                    continue;

                if (fix)
                    project.PackageDependencies.Remove(dependency);

                results.Add(new AuditResult(fix,
                    $"Useless package {dependency} reference, already used by: {string.Join(", ", foundDependencies.Select(e=>e.Name))}"
                ));
            }

            if (fix && results.Count > 0)
                project.Save();

            return results;
        }

        private IProject[] FindProjectDependenciesDeep(IProject project, Func<IProject, bool> isMatch)
        {
            if (project.ProjectDependencies.Count == 0)
                return Array.Empty<IProject>();

            var matches = new List<IProject>();
            foreach (var dependency in project.ProjectDependencies)
            {
                if (isMatch(dependency))
                    matches.Add(dependency);

                var nestedMatches = FindProjectDependenciesDeep(dependency, isMatch);
                if (nestedMatches.Length > 0)
                    matches.AddRange(nestedMatches);
            }

            return matches.Distinct().ToArray();
        }
    }
}