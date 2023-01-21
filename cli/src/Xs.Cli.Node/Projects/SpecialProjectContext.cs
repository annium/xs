using System.Collections.Generic;
using Annium.Extensions.Shell;
using Annium.Logging.Abstractions;
using Xs.Cli.Core.Audit;
using Xs.Cli.Core.Logging;
using Xs.Cli.Core.Models;
using Xs.Cli.Core.Projects;
using Xs.Cli.Node.Tools;

namespace Xs.Cli.Node.Projects;

internal class SpecialProjectContext<TProject> : ProjectBaseContext<TProject> where TProject : SpecialProject<TProject>
{
    public IReadOnlyDictionary<string, string> Scripts { get; }
    public IEnumerable<IAuditRule<ISpecialProject>> AuditRules { get; }
    public SpecialConfiguration Config { get; }
    public ProjectMapper Mapper { get; }

    public SpecialProjectContext(
        ProjectType type,
        string name,
        Version version,
        string description,
        string directory,
        HashSet<Dependency<IProject>> projects,
        HashSet<Dependency<Package>> packages,
        IReadOnlyDictionary<string, string> scripts,
        IShell shell,
        LoggerConfiguration loggerConfiguration,
        ILogger<TProject> logger,
        IEnumerable<IAuditRule<ISpecialProject>> auditRules,
        SpecialConfiguration config,
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
        Scripts = scripts;
        AuditRules = auditRules;
        Config = config;
        Mapper = mapper;
    }
}