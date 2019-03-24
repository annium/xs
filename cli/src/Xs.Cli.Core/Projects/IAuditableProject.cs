using System.Threading;
using Xs.Cli.Core.Audit;

namespace Xs.Cli.Core.Projects
{
    public interface IAuditableProject : IProject
    {
        AuditResult[] Audit(IProject[] projects, bool fix, CancellationToken token);
    }
}