using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xs.Cli.Core.Audit;

namespace Xs.Cli.Core.Projects;

public interface IAuditableProject : IProject
{
    Task<IReadOnlyCollection<AuditResult>> AuditAsync(IProject[] projects, string[] rules, bool fix, CancellationToken ct);
}