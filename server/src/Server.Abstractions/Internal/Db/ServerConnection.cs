using Annium.linq2db.Extensions.Models;
using Annium.Logging.Abstractions;
using LinqToDB;
using LinqToDB.Data;
using Server.Shared.Domain.Interfaces;

namespace Server.Abstractions.Internal.Db;

internal class ServerConnection<TPackage, TPackageDependency> : DataConnection, ILogSubject<ServerConnection<TPackage, TPackageDependency>>
    where TPackage : class, IPackage<TPackageDependency>
    where TPackageDependency : class, IPackageDependency
{
    public ILogger<ServerConnection<TPackage, TPackageDependency>> Logger { get; }
    public ITable<TPackage> Packages { get; }

    public ServerConnection(
        Config<ServerConnection<TPackage, TPackageDependency>> config,
        ILogger<ServerConnection<TPackage, TPackageDependency>> logger
    ) : base(config.Options)
    {
        Logger = logger;
        Packages = this.GetTable<TPackage>();
    }
}