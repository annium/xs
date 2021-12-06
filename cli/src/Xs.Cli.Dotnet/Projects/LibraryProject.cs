using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Projects;
using SysFile = System.IO.File;

namespace Xs.Cli.Dotnet.Projects;

internal class LibraryProject : SpecialProject<LibraryProject>, IPublishableProject
{
    public LibraryProject(SpecialProjectContext<LibraryProject> context) : base(context)
    {
    }

    public async Task<string> PackAsync(Core.Models.Version version, CancellationToken ct)
    {
        var file = Path.Combine(Directory, $"{Name}.{version}.nupkg");
        if (SysFile.Exists(file))
            SysFile.Delete(file);

        SetVersion(version);
        Save();

        var cmd = new List<string>();
        cmd.Add($"dotnet pack {File}");
        cmd.Add($"--output .");
        cmd.Add($"-p:PackageVersion={version}");
        cmd.Add($"-p:SymbolPackageFormat=snupkg");

        await RunAsync("pack", string.Join(' ', cmd), ct);

        return file;
    }

    public async Task PublishAsync(Uri registry, string accessToken, Core.Models.Version version, CancellationToken ct)
    {
        var packageFile = await PackAsync(version, ct);

        var source = registry.IsFile ? registry.AbsolutePath : new Uri(registry, Constants.ServerPathSuffix).ToString();

        var cmd = $"dotnet nuget push {packageFile} --source {source}";
        if (!registry.IsFile)
            cmd += $" --api-key {accessToken}";

        await RunAsync("publish", cmd, ct);

        System.IO.File.Delete(packageFile);
    }
}