using System.Collections.Generic;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit
{
    public interface IAuditRule<TProject> : IAuditRule where TProject : IProject
    {
        IEnumerable<AuditResult> Execute(IProject[] projects, TProject project, bool fix);
    }

    public interface IAuditRule
    {
        string Code { get; }
        
        string Description { get; }
    }
}