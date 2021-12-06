using System.Collections.Generic;
using System.Linq;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit;

public class FindInconsistentDependenciesRule<TProject> : IAuditRule<TProject> where TProject : IProject
{
    public string Code => "deps-consist";
    public string Description => "Finds inconsistent dependencies across projects. Fix uses latest for all projects";

    public IEnumerable<AuditResult> Execute(IProject[] projects, TProject project, bool fix)
    {
        var results = new List<AuditResult>();

        var version = projects.Max(p => p.Version)!;
        if (project.Version != version)
        {
            if (fix)
                project.Version.Update(version);

            results.Add(new AuditResult(fix,
                $"Project {project} uses lower version {project.Version} than {version}, used by others: {string.Join(", ", projects.Where(p => p.Version == version))}"
            ));
        }

        var packages = projects.SelectMany(p => p.Packages).Select(d => d.Value)
            .OrderBy(d => d.Name).ThenByDescending(d => d.Version)
            .Distinct().ToArray();

        foreach (var package in project.Packages.ToArray())
            results.AddRange(AuditPackage(packages, package, project, fix));

        if (fix && results.Count > 0)
            project.Save();

        return results;
    }

    private IEnumerable<AuditResult> AuditPackage(
        Package[] packages,
        Dependency<Package> package,
        TProject project,
        bool fix
    )
    {
        var(_, name, version) = package.Value;
        var nameLow = name.ToLowerInvariant();

        var others = packages.Where(p => p.Name.ToLowerInvariant() == nameLow).ToArray();
        if (others.Length == 0)
            yield break;

        var correctName = others.First().Name;
        if (name != correctName)
        {
            if (fix)
            {
                project.Packages.Remove(package);
                package = new Dependency<Package>(package.Type, new Package(package.Value.Type, correctName, package.Value.Version));
                project.Packages.Add(package);
            }

            yield return new AuditResult(fix,
                $"Project {project} uses different package naming: {project.Name} != {correctName}."
            );
        }

        var correctVersion = others.Max(p => p.Version)!;
        if (version != correctVersion)
        {
            if (fix)
            {
                project.Packages.Remove(package);
                package = new Dependency<Package>(package.Type, new Package(package.Value.Type, package.Value.Name, correctVersion));
                project.Packages.Add(package);
            }

            yield return new AuditResult(fix,
                $"Project {project} uses different package {name} version: {version} != {correctVersion}."
            );
        }
    }
}