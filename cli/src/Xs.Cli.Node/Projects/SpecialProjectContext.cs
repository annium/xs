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
        public IReadOnlyDictionary<string, string> Scripts { get; }

        public IEnumerable<IAuditRule<ISpecialProject>> AuditRules { get; }

        public ProjectMapper Mapper { get; }

        public SpecialProjectContext(
            ProjectType type,
            string name,
            Version version,
            string description,
            FileInfo file,
            HashSet<Dependency<IProject>> projects,
            HashSet<Dependency<Package>> packages,
            IReadOnlyDictionary<string, string> scripts,
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
            projects,
            packages,
            shell,
            loggerConfiguration,
            logger
        )
        {
            Scripts = scripts;
            AuditRules = auditRules;
            Mapper = mapper;
        }
    }
}