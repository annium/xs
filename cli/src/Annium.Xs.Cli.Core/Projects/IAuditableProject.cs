using System.Collections.Generic;
using System.Threading;
using Annium.Xs.Cli.Core.Audit;

namespace Annium.Xs.Cli.Core.Projects;

public interface IAuditableProject : IProject
{
    IReadOnlyCollection<AuditResult> Audit(
        IReadOnlyCollection<IProject> projects,
        string[] rules,
        bool fix,
        CancellationToken ct
    );
}
