using System.Collections.Generic;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Dotnet.Models;

namespace Xs.Cli.Dotnet.Projects;

internal class SpecialProjectContext<TProject> : ProjectBaseContext<TProject> where TProject : SpecialProject<TProject>
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
        string directory,
        HashSet<Dependency<IProject>> projects,
        HashSet<Dependency<Package>> packages,
        IShell shell,
        LoggerConfiguration loggerConfiguration,
        ILogger<TProject> logger,
        TargetFramework targetFramework,
        OutputType outputType,
        IEnumerable<IAuditRule<ISpecialProject>> auditRules,
        ProjectMapper mapper
    ) : base(
        type,
        name,
        version,
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
        Mapper = mapper;
    }
}