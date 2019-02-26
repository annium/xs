using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;
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
            Version version,
            string description,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            TargetFramework targetFramework,
            OutputType outputType,
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger
        ) : base(
            name,
            version,
            description,
            file,
            projectDependencies,
            packageDependencies,
            targetFramework,
            outputType,
            auditRules,
            mapper,
            shell,
            logger
        ) { }

        public Task TestAsync(Env env, CancellationToken token)
        {
            var configuration = env == Env.Development ? "Debug" : "Release";

            return RunAsync(
                "test",
                $"dotnet test --configuration {configuration} --no-build {File.FullName} /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./lcov",
                token);
        }
    }
}