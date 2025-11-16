using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;

namespace Annium.Xs.Cli.Dotnet.Projects;

internal class TestProject : PlatformProject, ITestableProject
{
    public TestProject(PlatformProjectContext context)
        : base(context) { }

    public Task TestAsync(Env env, string filter, CancellationToken ct)
    {
        var configuration = env == Env.Development ? "Debug" : "Release";

        var cmd = new List<string> { "dotnet test", $"--configuration {configuration}", $"--no-build {File}" };

        if (Packages.Any(d => d.Value.Name == ProjectFactory.TestCoveragePackage))
            cmd.AddRange([
                "/p:CollectCoverage=true",
                "/p:CoverletOutputFormat=lcov",
                "/p:CoverletOutput=./lcov",
                "--",
                $"logLevel={Enum.GetName(typeof(LogLevel), (LogLevel)LoggerConfiguration)!.ToLowerInvariant()}",
            ]);

        if (!string.IsNullOrWhiteSpace(filter))
            cmd.Add($"filter={filter}");

        return RunAsync("test", string.Join(' ', cmd), ct);
    }
}
