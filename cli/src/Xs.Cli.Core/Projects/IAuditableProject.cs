using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xs.Cli.Core.Projects
{
    public interface IAuditableProject : IProject
    {
        Task<IEnumerable<string>> AuditAsync(CancellationToken token);
    }
}