using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class TestProject : LibraryProject, ITestableProject
    {
        public TestProject(
            string name,
            FileInfo file,
            TargetFramework targetFramework,
            OutputType outputType,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger
        ) : base(
            name,
            file,
            targetFramework,
            outputType,
            projectDependencies,
            packageDependencies,
            mapper,
            shell,
            logger
        ) { }

        public async Task TestAsync(Env env, CancellationToken token)
        {
            logger.LogInfo($"Testing {Name}");

            var configuration = env == Env.Development ? "Debug" : "Release";
            var result = await shell.RunAsync(
                $"dotnet test --configuration {configuration} --no-build {File.FullName}",
                token);

            if (result.Code == 0)
                logger.LogInfo($"Tested {Name}");
            else
                throw new Exception($"Failed to test {Name}:{Environment.NewLine}{result.Output}");
        }
    }
}