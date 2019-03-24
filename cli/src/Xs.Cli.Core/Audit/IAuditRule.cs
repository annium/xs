using System.Collections.Generic;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit
{
    public interface IAuditRule<TProject> where TProject : IProject
    {
        IEnumerable<AuditResult> Execute(IProject[] projects, TProject project, bool fix);
    }
}