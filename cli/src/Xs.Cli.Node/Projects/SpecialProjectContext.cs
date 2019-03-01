using System.Collections.Generic;
using System.IO;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;

namespace Xs.Cli.Node.Projects
{
    internal class SpecialProjectContext : ProjectBaseContext
    {
        public IEnumerable<IAuditRule<ISpecialProject>> AuditRules { get; }

        public ProjectMapper Mapper { get; }

        public SpecialProjectContext(
            ProjectType type,
            string name,
            Version version,
            string description,
            FileInfo file,
            HashSet<IProject> projectDependencies,
            HashSet<Dependency> packageDependencies,
            IShell shell,
            LoggerConfiguration loggerConfiguration,
            ILogger logger,
            IEnumerable<IAuditRule<ISpecialProject>> auditRules,
            ProjectMapper mapper
        ) : base(
            type,
            name,
            version,
            description,
            file,
            projectDependencies,
            packageDependencies,
            shell,
            loggerConfiguration,
            logger
        )
        {
            AuditRules = auditRules;
            Mapper = mapper;
        }
    }
}