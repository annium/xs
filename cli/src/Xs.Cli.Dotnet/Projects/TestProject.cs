using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Dotnet.Projects
{
    internal class TestProject : BaseProject, ITestableProject
    {
        public TestProject(SpecialProjectContext context) : base(context) { }

        public Task TestAsync(Env env, CancellationToken token)
        {
            var configuration = env == Env.Development ? "Debug" : "Release";

            var cmd = new List<string>()
            {
                "dotnet test",
                $"--configuration {configuration}",
                $"--no-build {File.FullName}",
            };

            if (ProjectDependencies.Any(d => d.Name == ProjectFactory.TestCoveragePackage))
                cmd.AddRange(new []
                {
                    "/p:CollectCoverage=true",
                    "/p:CoverletOutputFormat=lcov",
                    "/p:CoverletOutput=./lcov",
                    "--",
                    $"logLevel={Enum.GetName(typeof(LogLevel),loggerConfiguration.LogLevel).ToLowerInvariant()}"
                });

            return RunAsync("test", string.Join(' ', cmd), token);
        }
    }
}