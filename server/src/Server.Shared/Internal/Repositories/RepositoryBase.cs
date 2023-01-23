using System;
using System.Threading.Tasks;
using LinqToDB.Data;

namespace Server.Shared.Internal.Repositories;

internal abstract class RepositoryBase<TConnection> : IAsyncDisposable
    where TConnection : DataConnection
{
    protected readonly TConnection Db;
    private bool _isDisposed;

    protected RepositoryBase(TConnection db)
    {
        Db = db;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        await Db.DisposeAsync();
    }
}