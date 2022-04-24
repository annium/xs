using System.Collections.Generic;
using System.Threading.Tasks;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit;

public interface IAuditRule<TProject> : IAuditRule where TProject : IProject
{
    Task<IReadOnlyCollection<AuditResult>> ExecuteAsync(IProject[] projects, TProject project, bool fix);
}

public interface IAuditRule
{
    string Code { get; }
    string Description { get; }
}