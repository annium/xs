using System.Collections.Generic;
using System.IO;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Core.Tools;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects
{
    internal class SpecialProjectContext : ProjectBaseContext
    {
        public TargetFramework TargetFramework { get; }

        public OutputType OutputType { get; }

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
            TargetFramework targetFramework,
            OutputType outputType,
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
            TargetFramework = targetFramework;
            OutputType = outputType;
            AuditRules = auditRules;
            Mapper = mapper;
        }
    }
}