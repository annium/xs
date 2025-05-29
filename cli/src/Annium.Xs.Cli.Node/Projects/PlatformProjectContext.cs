using System.Collections.Generic;
using Annium.Extensions.Shell;
using Annium.Logging;
using Annium.Xs.Cli.Core.Audit;
using Annium.Xs.Cli.Core.Logging;
using Annium.Xs.Cli.Core.Models;
using Annium.Xs.Cli.Core.Projects;
using Annium.Xs.Cli.Node.Tools;

namespace Annium.Xs.Cli.Node.Projects;

internal class PlatformProjectContext : ProjectBaseContext
{
    public IReadOnlyDictionary<string, string> Scripts { get; }
    public IEnumerable<IAuditRule<IPlatformProject>> AuditRules { get; }
    public PlatformConfiguration Config { get; }
    public ProjectMapper Mapper { get; }

    public PlatformProjectContext(
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
        ILogger logger,
        IEnumerable<IAuditRule<IPlatformProject>> auditRules,
        PlatformConfiguration config,
        ProjectMapper mapper
    )
        : base(type, name, version, description, directory, projects, packages, shell, loggerConfiguration, logger)
    {
        Scripts = scripts;
        AuditRules = auditRules;
        Config = config;
        Mapper = mapper;
    }
}
