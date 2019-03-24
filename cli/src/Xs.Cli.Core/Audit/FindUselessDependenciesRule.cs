using System;
using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit
{
    public class FindUselessDependenciesRule<TProject> : IAuditRule<TProject> where TProject : IProject
    {
        public string Code { get; } = "useless-deps";

        public string Description { get; } = "Finds useless dependencies in projects. Fix deletes useless deps";

        public IEnumerable<AuditResult> Execute(IProject[] projects, TProject project, bool fix)
        {
            var results = new List<AuditResult>();

            // check project dependencies
            foreach (var dependency in project.Projects.ToArray())
            {
                var foundDependencies = FindProjectDependenciesDeep(project, p => p.Projects.Contains(dependency));
                if (foundDependencies.Length == 0)
                    continue;

                if (fix)
                    project.Projects.Remove(dependency);

                results.Add(new AuditResult(fix,
                    $"Useless project {dependency} reference, already used by: {string.Join(", ", foundDependencies.Select(e=>e.Name))}"
                ));
            }

            // check package dependencies
            foreach (var dependency in project.Packages.ToArray())
            {
                var foundDependencies = FindProjectDependenciesDeep(project, p => p.Packages.Contains(dependency));
                if (foundDependencies.Length == 0)
                    continue;

                if (fix)
                    project.Packages.Remove(dependency);

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
            if (project.Projects.Count == 0)
                return Array.Empty<IProject>();

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
}