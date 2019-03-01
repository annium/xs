using System;
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

            var cmd = string.Join(' ', new string[]
            {
                "dotnet test",
                $"--configuration {configuration}",
                $"--no-build {File.FullName}",
                "/p:CollectCoverage=true",
                "/p:CoverletOutputFormat=lcov",
                "/p:CoverletOutput=./lcov",
                "--",
                $"logLevel={Enum.GetName(typeof(LogLevel),loggerConfiguration.LogLevel).ToLowerInvariant()}"
            });

            return RunAsync("test", cmd, token);
        }
    }
}