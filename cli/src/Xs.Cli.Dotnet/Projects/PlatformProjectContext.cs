using System.Collections.Generic;
using Annium.Extensions.Shell;
using Annium.Logging;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Models;
using Xs.Cli.Dotnet.Tools;

namespace Xs.Cli.Dotnet.Projects;

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
        Version version,
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
        : base(type, name, version, description, directory, projects, packages, shell, loggerConfiguration, logger)
    {
        TargetFramework = targetFramework;
        OutputType = outputType;
        AuditRules = auditRules;
        Config = config;
        Solutions = solutions;
        Mapper = mapper;
    }
}
