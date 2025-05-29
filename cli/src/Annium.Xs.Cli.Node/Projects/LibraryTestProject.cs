using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Version = Annium.Xs.Cli.Core.Models.Version;

namespace Annium.Xs.Cli.Node.Projects;

// TODO: rewrite into single project view
internal class LibraryTestProject : PlatformProject, IPublishableProject, ITestableProject
{
    public LibraryTestProject(PlatformProjectContext context)
        : base(context) { }

    public async Task<string> PackAsync(Version version, CancellationToken ct)
    {
        await InstallAsync(false, ct);
        await BuildAsync(Env.Production, true, ct);

        var fileName = $"{Name}-{version}.tgz";
        if (Name.StartsWith('@'))
        {
            var parts = Name[1..].Split('/');
            fileName = $"{parts[0]}-{parts[1]}-{version}.tgz";
        }

        var file = Path.Combine(Directory, fileName);
        if (System.IO.File.Exists(file))
            System.IO.File.Delete(file);

        // for NPM, project dependencies are not swapped with package dependencies when packaged, so need to do that manually
        var projectDependencies = Projects.ToArray();
        try
        {
            SetVersion(version);
            Projects.Clear();
            foreach (var (type, dependency) in projectDependencies)
                Packages.Add(
                    new Dependency<Package>(type, new Package(Constants.ProjectType, dependency.Name, version))
                );

            Save();

            await RunAsync("pack", $"npm pack {Directory}", ct);
        }
        finally
        {
            foreach (var dependency in projectDependencies)
            {
                Projects.Add(dependency);
                Packages.RemoveWhere(d => d.Type == dependency.Type && d.Value.Name == dependency.Value.Name);
            }

            Save();
        }

        return file;
    }

    public async Task PublishAsync(Uri registry, string accessToken, Version version, CancellationToken ct)
    {
        var packageFile = await PackAsync(version, ct);

        // due to NPM limitations, basically allowing single registry per scope, registry here is missing
        // instead, registry is specified in .npmrc
        await RunAsync("publish", $"npm publish {packageFile}", ct);

        System.IO.File.Delete(packageFile);
    }

    public Task TestAsync(Env env, string filter, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(filter)
            ? RunAsync("test", "pnpm test", ct)
            : RunAsync("test", $"pnpm test --testNamePattern {filter}", ct);
}
