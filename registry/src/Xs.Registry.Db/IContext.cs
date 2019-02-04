using System.Threading;
using System.Threading.Tasks;

namespace Xs.Registry.Db
{
    internal interface IContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}