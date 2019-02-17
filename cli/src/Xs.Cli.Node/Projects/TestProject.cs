using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Projects
{
    internal class TestProject : LibraryProject, ITestableProject
    {
        public TestProject(
            string name,
            Version version,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            ProjectMapper mapper,
            IShell shell,
            ILogger logger
        ) : base(
            name,
            version,
            file,
            projectDependencies,
            packageDependencies,
            mapper,
            shell,
            logger
        ) { }

        public Task TestAsync(Env env, CancellationToken token) =>
            RunAsync("test", $"yarn run test", token);
    }
}