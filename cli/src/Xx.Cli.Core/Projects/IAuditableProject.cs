using System.Collections.Generic;
using System.Threading;
using Xx.Cli.Core.Audit;

namespace Xx.Cli.Core.Projects;

public interface IAuditableProject : IProject
{
    IReadOnlyCollection<AuditResult> Audit(
        IReadOnlyCollection<IProject> projects,
        string[] rules,
        bool fix,
        CancellationToken ct
    );
}
