using System.Threading;
using System.Threading.Tasks;
using LinqToDB.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Xs.Registry.Db
{
    internal interface IContext
    {
        DataConnection GetDataConnection();

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}