using System.Collections.Generic;
using Annium.Extensions.Shell;
using Annium.Logging;
using Xx.Cli.Core.Audit;
using Xx.Cli.Core.Logging;
using Xx.Cli.Core.Models;
using Xx.Cli.Core.Projects;
using Xx.Cli.Dotnet.Models;
using Xx.Cli.Dotnet.Tools;

namespace Xx.Cli.Dotnet.Projects;

internal class PlatformProjectContext : ProjectBaseContext
{
    public PlatformConfiguration Config { get; }
    public IReadOnlyCollection<string> Solutions { get; }
    public TargetFramework TargetFramework { get; }
    public OutputType OutputType { get; }
    public IEnumerable<IAuditRule<IPlatformProject>> AuditRules { get; }
    public ProjectMapper Mapper { get; }

    public PlatformProjectContext(
        ProjectType type,
        string name,
        string description,
        string directory,
        HashSet<Dependency<IProject>> projects,
        HashSet<Dependency<Package>> packages,
        PlatformConfiguration config,
        IReadOnlyCollection<string> solutions,
        TargetFramework targetFramework,
        OutputType outputType,
        IEnumerable<IAuditRule<IPlatformProject>> auditRules,
        ProjectMapper mapper,
        IShell shell,
        LoggerConfiguration loggerConfiguration,
        ILogger logger
    )
        : base(
            type,
            name,
            new Version(0, 0, 0, string.Empty),
            description,
            directory,
            projects,
            packages,
            shell,
            loggerConfiguration,
            logger
        )
    {
        TargetFramework = targetFramework;
        OutputType = outputType;
        AuditRules = auditRules;
        Config = config;
        Solutions = solutions;
        Mapper = mapper;
    }
}
