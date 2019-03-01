using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Projects
{
    internal class TestProject : BaseProject, ITestableProject
    {
        public TestProject(
            string name,
            Version version,
            string description,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper,
            IShell shell,
            LoggerConfiguration loggerConfiguration,
            ILogger logger
        ) : base(
            name,
            version,
            description,
            file,
            projectDependencies,
            packageDependencies,
            auditRules,
            mapper,
            shell,
            loggerConfiguration,
            logger
        ) { }

        public Task TestAsync(Env env, CancellationToken token) =>
            RunAsync("test", $"yarn run test", token);
    }
}