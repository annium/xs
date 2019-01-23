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
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger,
            TargetFramework targetFramework,
            OutputType outputType
        ) : base(
            name,
            file,
            projectDependencies,
            packageDependencies,
            mapper,
            shell,
            logger,
            targetFramework,
            outputType
        ) { }

        public Task TestAsync(Env env, CancellationToken token)
        {
            var configuration = env == Env.Development ? "Debug" : "Release";

            return RunAsync(
                "test",
                $"dotnet test --configuration {configuration} --no-build {File.FullName}",
                token);
        }
    }
}