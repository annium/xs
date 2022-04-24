using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.Runtime.Loader;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Projects;

namespace Xs.Cli.Dotnet.Audit;

internal class FindUnusedDependenciesRule<TProject> : IAuditRule<TProject> where TProject : ISpecialProject
{
    public string Code => "unused-deps";
    public string Description => "Finds unused dependencies in .NET projects. Doesn't have fix for now due to transient deps possibilities";
    private readonly IAssemblyLoaderBuilder _assemblyLoaderBuilder;

    public FindUnusedDependenciesRule(
        IAssemblyLoaderBuilder assemblyLoaderBuilder
    )
    {
        _assemblyLoaderBuilder = assemblyLoaderBuilder;
    }

    public async Task<IReadOnlyCollection<AuditResult>> ExecuteAsync(IProject[] projects, TProject project, bool fix)
    {
        var results = new List<AuditResult>();

        var assembly = LoadAssembly(project);

        var dependencies = project.Projects.Where(d => d.Type != DependencyType.Dev && d.Type != DependencyType.Peer).ToArray();

        // check project dependencies
        foreach (var dependency in dependencies)
        {
            // var foundDependencies = FindProjectDependenciesDeep(project, p => p.Projects.Contains(dependency));
            // if (foundDependencies.Length == 0)
            //     continue;
            //
            // if (fix)
            //     project.Projects.Remove(dependency);

            results.Add(new AuditResult(fix,
                $"Unused project {dependency} reference"
            ));
        }

        return results;
    }

    private Assembly LoadAssembly(TProject project)
    {
        var path = Path.Combine(project.Directory, "bin", "Debug", project.TargetFramework.ToString(), $"{project.Name}.dll");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Assembly file '{path}' missing.");

        var name = Path.GetFileNameWithoutExtension(path);
        var loader = _assemblyLoaderBuilder.UseFileSystemLoader(Path.GetDirectoryName(path)!).Build();

        return loader.Load(name);
    }

    // private async Task<IReadOnlyCollection<string>> LoadProjectTypes(IProject project)
    // {
    //     if (project.Projects.Count == 0)
    //         return Array.Empty<IProject>();
    //
    //     var matches = new List<IProject>();
    //     foreach (var dependency in project.Projects)
    //     {
    //         if (isMatch(dependency.Value))
    //             matches.Add(dependency.Value);
    //
    //         var nestedMatches = FindProjectDependenciesDeep(dependency.Value, isMatch);
    //         if (nestedMatches.Length > 0)
    //             matches.AddRange(nestedMatches);
    //     }
    //
    //     return matches.Distinct().ToArray();
    // }
}