using System.Collections.Generic;
using Xs.Cli.Core.Projects;

namespace Xs.Cli.Core.Audit
{
    public interface IAuditRule<TProject> where TProject : IProject
    {
        IEnumerable<AuditResult> Execute(TProject project, bool fix);
    }
}