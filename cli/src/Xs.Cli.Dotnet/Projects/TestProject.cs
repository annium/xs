using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Dotnet.Projects
{
    internal class TestProject : SpecialProject<TestProject>, ITestableProject
    {
        public TestProject(SpecialProjectContext<TestProject> context) : base(context) { }

        public Task TestAsync(Env env, string filter, CancellationToken token)
        {
            var configuration = env == Env.Development ? "Debug" : "Release";

            var cmd = new List<string>()
            {
                "dotnet test",
                $"--configuration {configuration}",
                $"--no-build {File}",
            };

            if (Packages.Any(d => d.Value.Name == ProjectFactory.TestCoveragePackage))
                cmd.AddRange(new []
                {
                    "/p:CollectCoverage=true",
                    "/p:CoverletOutputFormat=lcov",
                    "/p:CoverletOutput=./lcov",
                    "--",
                    $"logLevel={Enum.GetName(typeof(LogLevel), (LogLevel)LoggerConfiguration)!.ToLowerInvariant()}"
                });

            if (!string.IsNullOrWhiteSpace(filter))
                cmd.Add($"filter={filter}");

            return RunAsync("test", string.Join(' ', cmd), token);
        }
    }
}